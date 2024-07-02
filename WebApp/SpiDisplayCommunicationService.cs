using System.Device.Gpio;
using System.Device.Spi;

namespace WebApp;

public class SpiDisplayCommunicationService : IDisplayCommunicationService
{
    private readonly ILogger<SpiDisplayCommunicationService> _logger;
    private readonly GpioController _gpioController;
    private SpiDevice _dataSpi;
    private GpioPin _latchPinFront;
    private GpioPin _latchPinBack;
    private byte _frameBufferIndex;
    private readonly object _lock = new();
    
    public int FrontLatchPin { get; set; } = 8;
    public int BackLatchPin  { get; set; } = 7;
    public int SpiClockFrequency  { get; set; } = 1000000;
    public bool SendToFront { get; set; } = true;
    public bool SendToBack { get; set; } = true;
    public bool InvertBack { get; set; } = true;

    public SpiDisplayCommunicationService(ILogger<SpiDisplayCommunicationService> logger)
    {
        _logger = logger;
        _gpioController = new GpioController();
        InitComm();
    }

    private void InitComm()
    {
        _dataSpi = SpiDevice.Create(new SpiConnectionSettings(0)
        {
            ClockFrequency = SpiClockFrequency,
            Mode = SpiMode.Mode2,
            DataBitLength = 8
        });
        
        _latchPinFront = _gpioController.OpenPin(FrontLatchPin, PinMode.Output);
        _latchPinFront.Write(PinValue.Low);
        
        _latchPinBack = _gpioController.OpenPin(BackLatchPin, PinMode.Output);
        _latchPinBack.Write(PinValue.Low);
    }

    public void RestartComm()
    {
        lock (_lock)
        {
            _gpioController.ClosePin(_latchPinFront.PinNumber);
            _gpioController.ClosePin(_latchPinBack.PinNumber);
            _dataSpi.Dispose();
            InitComm();
        }
    }
    
    public void SendImage(DisplayImage image)
    {
        lock (_lock)
        {
            SendImageInternal(image);
        }
    }

    private void SendImageInternal(DisplayImage image)
    {
        var payload = image.GetPayload(_frameBufferIndex).ToArray();
        _logger.LogInformation($"Sending {payload.Length} Bytes to Display");

        if (SendToFront)
        {
            foreach (var chunk in payload.Chunk(1740))
            {
                _dataSpi.Write(chunk);
                Task.Delay(TimeSpan.FromTicks(10)).Wait();
                _latchPinFront.Write(PinValue.High);
                Task.Delay(TimeSpan.FromTicks(30)).Wait();
                _latchPinFront.Write(PinValue.Low);
                Task.Delay(TimeSpan.FromTicks(10)).Wait();
            }
        }

        if (SendToBack)
        {
            if (InvertBack)
            {
                payload = image.GetInvertedPayload(_frameBufferIndex).ToArray();
            }
            
            foreach (var chunk in payload.Chunk(1740))
            {
                _dataSpi.Write(chunk);
                Task.Delay(TimeSpan.FromTicks(10)).Wait();
                _latchPinBack.Write(PinValue.High);
                Task.Delay(TimeSpan.FromTicks(30)).Wait();
                _latchPinBack.Write(PinValue.Low);
                Task.Delay(TimeSpan.FromTicks(10)).Wait();
            }
        }

        _frameBufferIndex ^= 1;
    }
}