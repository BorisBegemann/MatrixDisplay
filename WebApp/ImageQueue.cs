using System.Collections.Concurrent;

namespace WebApp;

public class ImageQueue
{
    private readonly ConcurrentQueue<DisplayImage> _queue  = new();

    public void EnqueueImage(DisplayImage image) => _queue.Enqueue(image);
    
    public bool TryDequeueImage(out DisplayImage image) => _queue.TryDequeue(out image!);
}