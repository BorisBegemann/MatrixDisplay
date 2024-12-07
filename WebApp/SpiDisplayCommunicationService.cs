using System.Device.Gpio;
using System.Device.Spi;

namespace WebApp;

public interface ISpiDisplayCommunicationService
{
}

public class SpiDisplayCommunicationService : IDisplayCommunicationService, ISpiDisplayCommunicationService
{
    private readonly ILogger<SpiDisplayCommunicationService> _logger;
    private GpioController? _gpioController;
    private SpiDevice? _dataSpi;
    private GpioPin? _latchPinFront;
    private GpioPin? _latchPinBack;
    private byte _frameBufferIndex;
    private readonly object _lock = new();
    
    public int FrontLatchPin { get; set; } = 8;
    public int BackLatchPin  { get; set; } = 7;
    public int LatchDelayInTicks { get; set; } = 10;
    public int LatchDurationInTicks { get; set; } = 30;
    public int SpiClockFrequency  { get; set; } = 1000000;
    public bool SendToFront { get; set; } = true;
    public bool SendToBack { get; set; } = true;
    public bool InvertBack { get; set; } = true;

    public SpiDisplayCommunicationService(ILogger<SpiDisplayCommunicationService> logger)
    {
        _logger = logger;
        InitComm();
    }

    private void InitComm()
    {
        try
        {
            _dataSpi = SpiDevice.Create(new SpiConnectionSettings(0)
            {
                ClockFrequency = SpiClockFrequency,
                Mode = SpiMode.Mode2,
                DataBitLength = 8
            });

            _gpioController = new GpioController();
            _latchPinFront = _gpioController.OpenPin(FrontLatchPin, PinMode.Output);
            _latchPinFront.Write(PinValue.Low);

            _latchPinBack = _gpioController.OpenPin(BackLatchPin, PinMode.Output);
            _latchPinBack.Write(PinValue.Low);
        }
        catch (PlatformNotSupportedException)
        {
            _logger.LogError("Error initializing Hardware - Platform not supported");
        }
    }

    public void RestartComm()
    {
        lock (_lock)
        {
            if (_latchPinFront != null) _gpioController?.ClosePin(_latchPinFront.PinNumber);
            if (_latchPinBack != null) _gpioController?.ClosePin(_latchPinBack.PinNumber);
            _dataSpi?.Dispose();
            _gpioController?.Dispose();
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
        if (_dataSpi == null || _latchPinBack == null || _latchPinFront == null)
        {
            _logger.LogError($"Hardware not initialized");
            return;
        }
        
        var payload = image.GetPayload(_frameBufferIndex);
        _logger.LogInformation($"Sending {payload.Length} Bytes to Display");

        if (SendToFront)
        {
            foreach (var chunk in payload.Chunk(1740))
            {
                _dataSpi.Write(chunk);
                Task.Delay(TimeSpan.FromTicks(LatchDelayInTicks)).Wait();
                _latchPinFront.Write(PinValue.High);
                Task.Delay(TimeSpan.FromTicks(LatchDurationInTicks)).Wait();
                _latchPinFront.Write(PinValue.Low);
                Task.Delay(TimeSpan.FromTicks(LatchDelayInTicks)).Wait();
            }
        }

        if (SendToBack)
        {
            if (InvertBack)
            {
                payload = image.GetInvertedPayload(_frameBufferIndex);
            }
            
            foreach (var chunk in payload.Chunk(1740))
            {
                _dataSpi.Write(chunk);
                Task.Delay(TimeSpan.FromTicks(LatchDelayInTicks)).Wait();
                _latchPinBack.Write(PinValue.High);
                Task.Delay(TimeSpan.FromTicks(LatchDurationInTicks)).Wait();
                _latchPinBack.Write(PinValue.Low);
                Task.Delay(TimeSpan.FromTicks(LatchDelayInTicks)).Wait();
            }
        }

        _frameBufferIndex ^= 1;
    }
}