﻿using Microsoft.AspNetCore.Mvc;
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
    private readonly ImageManager.ImageManager _imageManager;

    public Custom(IHostEnvironment environment, ImageQueue queue, ImageManager.ImageManager imageManager)
    {
        _environment = environment;
        _queue = queue;
        _imageManager = imageManager;
    }
    
    public IReadOnlyList<Guid> GetAvailableImages()
    {
        return _imageManager.ListImages();
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
    public IFormFile? Upload { get; set; }
    
    public async Task OnPostAsync()
    {
        if (Upload is null)
        {
            return;
        }
        
        var file = Path.Combine(_environment.ContentRootPath, "uploads", Guid.NewGuid().ToString());
        using var img = await Image.LoadAsync(Upload.OpenReadStream());
        
        if (img.Width != 580 || img.Height != 104)
        {
            return;
        }

        await _imageManager.SaveImage(img);
    }
    
    public IActionResult OnPostShowImage(string imageName)
    {
        using var img = Image.Load(Path.Combine(_environment.ContentRootPath, "uploads", imageName));
        var displayImage = new DisplayImage(img.CloneAs<Rgb24>());
        _queue.EnqueueImage(displayImage);
        return new EmptyResult();
    }
}