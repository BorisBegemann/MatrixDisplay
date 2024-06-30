namespace WebApp;

class NullDisplayCommunicationService : IDisplayCommunicationService
{
    private readonly ILogger<NullDisplayCommunicationService> _logger;
    
    public NullDisplayCommunicationService(ILogger<NullDisplayCommunicationService> logger)
    {
        _logger = logger;
    }

    public int FrontLatchPin { get; set; } = 8;
    public int BackLatchPin  { get; set; } = 7;
    public int SpiClockFrequency  { get; set; } = 1000000;
    public bool SendToFront { get; set; } = true;
    public bool SendToBack { get; set; } = false;
    public bool InvertBack { get; set; } = false;

    public void SendImage(DisplayImage image)
    {
        _logger.LogInformation("Image received");
    }

    public void RestartComm()
    {
        _logger.LogInformation("Comm restarted");
    }
}