using Microsoft.AspNetCore.ResponseCompression;
using MudBlazor.Services;
using WebApp.Components;
using WebApp.Hubs;

namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        builder.Services.AddSignalR();
        builder.Services.AddResponseCompression(opts =>
        {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                ["application/octet-stream"]);
        });
        
        builder.Services.AddMudServices();
        builder.Services.AddControllers();
        builder.Services.AddSingleton<ImageQueue>();
        builder.Services.AddSingleton<ImageManager.ImageManager>();
        builder.Services.AddSingleton<DisplayManager.DisplayManager>();
        builder.Services.AddSingleton<IDisplayCommunicationService, SpiDisplayCommunicationService>();
        builder.Services.AddHostedService<ImageSender>();
        
        var app = builder.Build();
        app.UseResponseCompression();
        app.MapHub<ImageManagerHub>(ImageManagerHub.Path);
        app.MapHub<DisplayManagerHub>(DisplayManagerHub.Path);
        app.UseRouting();
        app.MapControllers();
        app.UseAntiforgery();
        app.MapRazorComponents<Display>()
            .AddInteractiveServerRenderMode();
        
        app.UseStaticFiles();
        app.Run();
    }
}