using System.Collections.Concurrent;

namespace WebApp.DisplayManager;

public class DisplayManager(
    IDisplayCommunicationService displayCommunicationService,
    ImageManager.ImageManager imageManager)
{
    private readonly ConcurrentQueue<Guid> _queue = new();
    private DisplayManagerImage? _currentImage;

    public void EnqueueSavedImage(Guid image)
    {
        _queue.Enqueue(image);
    }
    
    public Guid GetCurrentImageName()
    {
        return _currentImage?.Name ?? Guid.Empty;
    }
    
    public void DisplayNextImage()
    {
        if (_queue.TryDequeue(out var image))
        {
            var img = imageManager.GetImage(image);
            _currentImage = new DisplayManagerImage
            {
                Name = image,
                Image = new DisplayImage(img)
            };
            
            displayCommunicationService.SendImage(_currentImage.Image);
        }
    }
}

public class DisplayManagerImage
{
    public Guid Name { get; set; }
    public DisplayImage Image { get; set; }
}