using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApp.Pages;

[IgnoreAntiforgeryToken(Order = 1001)]
public class Custom : PageModel
{
    private readonly IHostEnvironment _environment;
    private readonly ImageQueue _queue;

    public Custom(IHostEnvironment environment, ImageQueue queue)
    {
        _environment = environment;
        _queue = queue;
    }
    
    public IReadOnlyList<string> GetAvailableImages()
    {
        var path = Path.Combine(_environment.ContentRootPath, "uploads");
        return Directory
            .GetFiles(path)
            .Select(Path.GetFileName)
            .Where(x => x != null)
            .ToList()!;
    }

    public IActionResult OnGetImage(string name)
    {
        var path = Path.Combine(_environment.ContentRootPath, "uploads", name);
        if (Path.Exists(path))
        {
            return PhysicalFile(path, "image/png");
        }
        
        return NotFound();
    }
    
    [BindProperty]
    public IFormFile Upload { get; set; }
    
    public async Task OnPostAsync()
    {
        var file = Path.Combine(_environment.ContentRootPath, "uploads", Guid.NewGuid().ToString());
        var img = await Image.LoadAsync(Upload.OpenReadStream());
        
        if (img.Width != 580 || img.Height != 104)
        {
            return;
        }

        await img.SaveAsync(file, new PngEncoder());
    }
    
    public IActionResult OnPostShowImage(string imageName)
    {
        var img = Image.Load(Path.Combine(_environment.ContentRootPath, "uploads", imageName));
        var displayImage = new DisplayImage(img.CloneAs<Rgb24>());
        _queue.EnqueueImage(displayImage);
        return new EmptyResult();
    }
}