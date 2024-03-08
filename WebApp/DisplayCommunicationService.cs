using System.Device.Gpio;
using System.Device.Spi;

namespace WebApp;

public class DisplayCommunicationService
{
    private readonly ILogger<DisplayCommunicationService> _logger;
    private readonly SpiDevice _dataSpi;
    private readonly SpiDevice _latchSpi;
    
    public DisplayCommunicationService(ILogger<DisplayCommunicationService> logger)
    {
        _logger = logger;
        _dataSpi = SpiDevice.Create(new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = 1000000,
            Mode = SpiMode.Mode0,
            DataBitLength = 8
        });
        _latchSpi = SpiDevice.Create(new SpiConnectionSettings(1, 0)
        {
            ClockFrequency = 1000000,
            Mode = SpiMode.Mode0,
            DataBitLength = 8
        });
    }
    
    public void SendImage(DisplayImage image)
    {
        var payload = image.GetPayload().ToArray();
        _logger.LogInformation($"Sending {payload.Length} Bytes to Display");
        foreach (var chunk in payload.Chunk(1740))
        {
            _dataSpi!.Write(chunk);
            _latchSpi.WriteByte(1);
        }
        
    }
}