using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApp;

public class ImageSender : IHostedService
{
    private Timer? _timer;
    
    private readonly TimeSpan _interval;
    private readonly DisplayManager.DisplayManager _displayManager;

    public ImageSender(DisplayManager.DisplayManager displayManager, IDisplayCommunicationService displayCommunicationService, ILogger<ImageSender> logger)
    {
        _interval = TimeSpan.FromMilliseconds(200000);
        _displayManager = displayManager;
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
        _timer?.Change(Timeout.Infinite, 0);
        _displayManager.DisplayNextImage();
        _timer?.Change(TimeSpan.Zero, _interval);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}