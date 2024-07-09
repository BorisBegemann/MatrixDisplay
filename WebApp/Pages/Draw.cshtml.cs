using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApp.Pages;

[IgnoreAntiforgeryToken(Order = 1001)]
public class DrawModel : PageModel
{
    private readonly IHostEnvironment _environment;
    private readonly ImageQueue _queue;
    private readonly ILogger<DrawModel> _logger;

    public DrawModel(ImageQueue queue, ILogger<DrawModel> logger, IHostEnvironment environment)
    {
        _queue = queue;
        _logger = logger;
        _environment = environment;
    }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostSend(string image)
    {
        _logger.LogInformation("Received Image");
        var data = Convert.FromBase64String(image[22..]);
        using var ms = new MemoryStream(data);
        using var img = await Image.LoadAsync(ms);
        var displayImage = new DisplayImage(img.CloneAs<Rgb24>());
        _queue.EnqueueImage(displayImage);
        return new EmptyResult();

    }

    public async Task<IActionResult> OnPostSave(string image)
    {
        _logger.LogInformation("Received Image");
        var data = Convert.FromBase64String(image[22..]);
        using var ms = new MemoryStream(data);
        using var img = await Image.LoadAsync(ms);
        var file = Path.Combine(_environment.ContentRootPath, "uploads", Guid.NewGuid().ToString());
        await img.SaveAsync(file, new PngEncoder());
        return new EmptyResult();
    }
}