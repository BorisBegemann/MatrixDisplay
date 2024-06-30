using System.Device.Gpio;
using System.Device.Spi;

namespace WebApp;

public class SpiDisplayCommunicationService : IDisplayCommunicationService
{
    private readonly ILogger<SpiDisplayCommunicationService> _logger;
    private readonly GpioController _gpioController;
    private readonly SpiDevice _dataSpi;
    private readonly GpioPin _latchPinFront;
    private readonly GpioPin _latchPinBack;
    private byte _frameBufferIndex = 0;
    public SpiDisplayCommunicationService(ILogger<SpiDisplayCommunicationService> logger)
    {
        _logger = logger;
        _dataSpi = SpiDevice.Create(new SpiConnectionSettings(0)
        {
            ClockFrequency = 1000000,
            Mode = SpiMode.Mode2,
            DataBitLength = 8
        });
        _gpioController = new GpioController();
        _latchPinFront = _gpioController.OpenPin(8, PinMode.Output);
        _latchPinFront.Write(PinValue.Low);
        
        _latchPinBack = _gpioController.OpenPin(7, PinMode.Output);
        _latchPinBack.Write(PinValue.Low);
    }
    
    public void SendImage(DisplayImage image)
    {
        var payload = image.GetPayload(_frameBufferIndex).ToArray();
        _logger.LogInformation($"Sending {payload.Length} Bytes to Display");
        
        foreach (var chunk in payload.Chunk(1740))
        {
            _dataSpi.Write(chunk);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
            _latchPinFront.Write(PinValue.High);
            Task.Delay(TimeSpan.FromTicks(30)).Wait();
            _latchPinFront.Write(PinValue.Low);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
        }
        
        Task.Delay(TimeSpan.FromTicks(200)).Wait();
        
        foreach (var chunk in payload.Chunk(1740))
        {
            _dataSpi.Write(chunk);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
            _latchPinBack.Write(PinValue.High);
            Task.Delay(TimeSpan.FromTicks(30)).Wait();
            _latchPinBack.Write(PinValue.Low);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
        }
        
        _frameBufferIndex ^= 1;
    }
}