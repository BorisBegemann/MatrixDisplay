namespace WebApp;

class NullDisplayCommunicationService : IDisplayCommunicationService
{
    private readonly ILogger<NullDisplayCommunicationService> _logger;
    
    public NullDisplayCommunicationService(ILogger<NullDisplayCommunicationService> logger)
    {
        _logger = logger;
    }

    public int FrontLatchPin { get; set; }
    public int BackLatchPin { get; set; }
    public int SpiClockFrequency { get; set; }
    public bool SendToFront { get; set; }
    public bool SendToBack { get; set; }
    public bool InvertBack { get; set; }

    public void SendImage(DisplayImage image)
    {
        _logger.LogInformation("Image received");
    }

    public void RestartComm()
    {
        _logger.LogInformation("Comm restarted");
    }
}