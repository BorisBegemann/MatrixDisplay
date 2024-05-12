using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApp.Pages;

[IgnoreAntiforgeryToken(Order = 1001)]
public class DrawModel : PageModel
{
    private readonly ImageQueue _queue;
    private readonly ILogger<DrawModel> _logger;

    public DrawModel(ImageQueue queue, ILogger<DrawModel> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        return Page();
    }
    
    public IActionResult OnPostImage(string image)
    {
        _logger.LogInformation("Received Image");
        var data = Convert.FromBase64String(image[22..]);
        using var ms = new MemoryStream(data);
        using var img = Image.Load(ms);
        var displayImage = new DisplayImage(img.CloneAs<Rgb24>());
        _queue.EnqueueImage(displayImage);
        return new EmptyResult();
    }
}