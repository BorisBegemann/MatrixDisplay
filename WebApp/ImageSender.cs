namespace WebApp;

public class ImageSender : IHostedService
{
    private Timer? _timer;
    
    private readonly TimeSpan _interval;
    private readonly ImageQueue _queue;
    private readonly DisplayCommunicationService _displayCommunicationService;
    private readonly Logger<ImageSender> _logger;

    public ImageSender(ImageQueue queue, DisplayCommunicationService displayCommunicationService, Logger<ImageSender> logger)
    {
        _interval = TimeSpan.FromMilliseconds(100);
        _queue = queue;
        _displayCommunicationService = displayCommunicationService;
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(
            SendNextImage, 
            null, 
            TimeSpan.Zero,
            _interval);

        return Task.CompletedTask;
    }

    private void SendNextImage(object? state)
    {
        if(_queue.TryDequeueImage(out var image))
        {
            _logger.LogInformation("Dequeued Image, Sent to Display");
            _displayCommunicationService.SendImage(image);
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}