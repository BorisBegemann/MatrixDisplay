using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;

namespace WebApp.Pages;

[IgnoreAntiforgeryToken(Order = 1001)]
public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
    
    public IActionResult OnPostImage(string image)
    {
        logger.LogError("Got called with Image");
        var data = Convert.FromBase64String(image[22..]);
        using (MemoryStream ms = new MemoryStream(data))
        {
            var img = Image.Load(ms);
        }

        return new EmptyResult();
    }
}