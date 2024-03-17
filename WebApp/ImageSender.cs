using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApp;

public class ImageSender : IHostedService
{
    private Timer? _timer;
    
    private readonly TimeSpan _interval;
    private readonly ImageQueue _queue;
    private readonly IDisplayCommunicationService _displayCommunicationService;
    private readonly ILogger<ImageSender> _logger;

    public ImageSender(ImageQueue queue, IDisplayCommunicationService displayCommunicationService, ILogger<ImageSender> logger)
    {
        _interval = TimeSpan.FromMilliseconds(2000);
        _queue = queue;
        _displayCommunicationService = displayCommunicationService;
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken stoppingToken)
    {
        var img = Image.Load("wwwroot/img/init.png");
        var displayImage = new DisplayImage(img.CloneAs<Rgb24>());
        _queue.EnqueueImage(displayImage);
        
        _timer = new Timer(
            SendNextImage, 
            null, 
            TimeSpan.Zero,
            _interval);

        return Task.CompletedTask;
    }

    private void SendNextImage(object? state)
    {
        _timer?.Change(Timeout.Infinite, 0);
        
        if(_queue.TryDequeueImage(out var image))
        {
            _logger.LogInformation("Dequeued Image, Sent to Display");
            _displayCommunicationService.SendImage(image);
        }
        
        _timer?.Change(TimeSpan.Zero, _interval);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}