using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.Formats.Png;

namespace WebApp.Api;

[ApiController]
[Route("api/[controller]/{name?}")]
public class ImageController(ImageManager.ImageManager imageManager) : Controller
{
    private static int[] _test = [1, 2, 3, 4, 5];

    [HttpGet]
    public async Task<IActionResult> Index(Guid name)
    {
        try
        {
            var img = imageManager.GetImage(name);
            var stream = new MemoryStream();
            await img.SaveAsync(stream, new PngEncoder());
            stream.Position = 0;
            return new FileStreamResult(stream, "image/png");
        }
        catch (Exception e)
        {
            return NotFound();
        }
        
    }
}