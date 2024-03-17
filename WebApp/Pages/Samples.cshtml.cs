using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WebApp.Pages;

[IgnoreAntiforgeryToken(Order = 1001)]
public class Samples : PageModel
{
    private readonly ImageQueue _queue;
    private readonly ILogger<Samples> _logger;

    public readonly List<string> SampleImages = new()
    {
        "chess1.png",
        "horiz.png",
        "poop.png",
        "test.png",
        "vert.png",
        "weinfelden.png",
        "white.png",
    };
    
    public Samples(ImageQueue queue, ILogger<Samples> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        return Page();
    }
    
    public IActionResult OnPostShowSample(int sampleIndex)
    {
        _logger.LogInformation("Received sample index {0}", sampleIndex);
        var imageName = SampleImages[sampleIndex];
        var img = Image.Load($"wwwroot/img/{imageName}");
        var displayImage = new DisplayImage(img.CloneAs<Rgb24>());
        _queue.EnqueueImage(displayImage);
        return new EmptyResult();
    }
}