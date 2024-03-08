using System.Device.Gpio;
using System.Device.Spi;

namespace WebApp;

public class DisplayCommunicationService
{
    private readonly ILogger<DisplayCommunicationService> _logger;
    private readonly SpiDevice _spi;
    private readonly GpioPin _latchPin;
    
    public DisplayCommunicationService(ILogger<DisplayCommunicationService> logger)
    {
        _logger = logger;
        _spi = SpiDevice.Create(new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = 1000000,
            Mode = SpiMode.Mode0,
            DataBitLength = 8
        });
        _latchPin = new GpioController().OpenPin(0, PinMode.Output);
        _latchPin.Write(PinValue.Low);
    }
    
    public void SendImage(DisplayImage image)
    {
        var payload = image.GetPayload().ToArray();
        _logger.LogInformation($"Sending {payload.Length} Bytes to Display");
        foreach (var chunk in payload.Chunk(1740))
        {
            _spi!.Write(chunk);
            _latchPin.Write(PinValue.High);
            Thread.Sleep(1);
            _latchPin.Write(PinValue.Low);
        }
        
    }
}