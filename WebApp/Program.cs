using Microsoft.AspNetCore.ResponseCompression;
using WebApp.Hubs;

namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        
        builder.Services.AddSignalR();
        builder.Services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                ["application/octet-stream"]);
        });
        
        builder.Services.AddSingleton<ImageQueue>();
        builder.Services.AddSingleton<ImageManager.ImageManager>();
        builder.Services.AddSingleton<IDisplayCommunicationService, SpiDisplayCommunicationService>();
        builder.Services.AddHostedService<ImageSender>();
        
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AddPageRoute("/Draw", "{*url}");
        });
        
        var app = builder.Build();
        app.UseResponseCompression();
        app.MapHub<ImageManagerHub>(ImageManagerHub.Path);
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();
        app.Run();
    }
}