using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApp.Pages;

[IgnoreAntiforgeryToken(Order = 1001)]
public class DrawModel(
    ImageQueue queue,
    ILogger<DrawModel> logger,
    ImageManager.ImageManager imageManager)
    : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostSend(string image)
    {
        logger.LogInformation("Received Image");
        var data = Convert.FromBase64String(image[22..]);
        using var ms = new MemoryStream(data);
        using var img = await Image.LoadAsync(ms);
        var displayImage = new DisplayImage(img.CloneAs<Rgb24>());
        queue.EnqueueImage(displayImage);
        return new EmptyResult();
    }

    public async Task<IActionResult> OnPostSave(string image)
    {
        logger.LogInformation("Received Image");
        var data = Convert.FromBase64String(image[22..]);
        using var ms = new MemoryStream(data);
        using var img = await Image.LoadAsync(ms);
        await imageManager.SaveImage(img);
        return new EmptyResult();
    }
}