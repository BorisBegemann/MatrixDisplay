namespace WebApp;

public class ImageSender : IHostedService
{
    private Timer? _timer;
    
    private readonly TimeSpan _interval;
    private readonly ImageQueue _queue;
    private readonly DisplayCommunicationService _displayCommunicationService;

    public ImageSender(ImageQueue queue, DisplayCommunicationService displayCommunicationService)
    {
        _interval = TimeSpan.FromSeconds(30);
        _queue = queue;
        _displayCommunicationService = displayCommunicationService;
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
            _displayCommunicationService.SendImage(image);
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}