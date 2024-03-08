using System.Device.Spi;

namespace WebApp;

public class DisplayCommunicationService
{
    private readonly SpiDevice? _spi;
    
    public DisplayCommunicationService(int busId, int chipSelectLine)
    {
        _spi = SpiDevice.Create(new SpiConnectionSettings(busId, chipSelectLine)
        {
            ClockFrequency = 500000,
            Mode = SpiMode.Mode0,
            DataBitLength = 8
        });
    }
    
    public void SendImage(DisplayImage image)
    {
        var payload = image.GetPayload().ToArray();
        _spi!.Write(payload);
    }
}