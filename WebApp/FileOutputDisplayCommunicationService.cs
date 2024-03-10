namespace WebApp;

public class FileOutputDisplayCommunicationService : IDisplayCommunicationService
{
    private readonly ILogger<FileOutputDisplayCommunicationService> _logger;

    public FileOutputDisplayCommunicationService(ILogger<FileOutputDisplayCommunicationService> logger)
    {
        _logger = logger;
    }
    
    public void SendImage(DisplayImage image)
    {
        var payload = image.GetPayload(1).ToArray();
        var fileName = Guid.NewGuid().ToString();
        var dataLines = payload.Chunk(6)
            .Select(c => $"\"SPI\", 0.00, 0x{string.Join("", c.Select(b => b.ToString("X2")))}, 0x000");
        var headerLine = "name,start_time,\"mosi\",\"miso\"";
        File.WriteAllLines($"{fileName}.csv", new []{ headerLine}.Concat(dataLines));
        _logger.LogInformation($"Wrote {payload.Count()} Bytes to {fileName}.csv");
    }
}