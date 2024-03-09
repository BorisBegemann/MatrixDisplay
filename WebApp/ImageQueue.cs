using System.Collections.Concurrent;

namespace WebApp;

public class ImageQueue
{
    private readonly ConcurrentQueue<DisplayImage> _queue  = new();

    public void EnqueueImage(DisplayImage image) => _queue.Enqueue(image);

    private bool _requeuedLastImage;
    
    public bool TryDequeueImage(out DisplayImage image)
    {
        var anyImage = _queue.TryDequeue(out image!);
        
        if (anyImage && _queue.IsEmpty)
        {
            _queue.Enqueue(image);
        }
        else if (anyImage && _requeuedLastImage)
        {
            _requeuedLastImage = false;
            _queue.TryDequeue(out image!);
        }

        return anyImage;
    }
}