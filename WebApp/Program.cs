namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        
        builder.Services.AddSingleton<ImageQueue>();
        
        builder.Services.AddSingleton<IDisplayCommunicationService, SpiDisplayCommunicationService>();
        //builder.Services.AddSingleton<IDisplayCommunicationService, NullDisplayCommunicationService>();
        builder.Services.AddHostedService<ImageSender>();
        
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AddPageRoute("/Draw", "{*url}");
        });
        var app = builder.Build();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();
        app.Run();
    }
}