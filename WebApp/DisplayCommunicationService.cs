using System.Device.Spi;

namespace WebApp;

public class DisplayCommunicationService
{
    private readonly ILogger<DisplayCommunicationService> _logger;
    private readonly SpiDevice? _spi;
    
    public DisplayCommunicationService(ILogger<DisplayCommunicationService> logger)
    {
        _logger = logger;
        _spi = SpiDevice.Create(new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = 500000,
            Mode = SpiMode.Mode0,
            DataBitLength = 8
        });
    }
    
    public void SendImage(DisplayImage image)
    {
        var payload = image.GetPayload().ToArray()[..254];
        _logger.LogInformation($"Sending {payload.Length} Bytes to Display");
        _spi!.Write(payload);
    }
}