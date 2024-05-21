namespace WebApp;

class NullDisplayCommunicationService : IDisplayCommunicationService
{
    private readonly ILogger<NullDisplayCommunicationService> _logger;
    
    public NullDisplayCommunicationService(ILogger<NullDisplayCommunicationService> logger)
    {
        _logger = logger;
    }
    
    public void SendImage(DisplayImage image)
    {
        _logger.LogInformation("Image received");
    }
}