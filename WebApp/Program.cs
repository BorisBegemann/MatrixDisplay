namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();
        builder.Services.AddSingleton<ImageQueue>();
        builder.Services.AddSingleton(new DisplayCommunicationService(0, 0));
        builder.Services.AddHostedService<ImageSender>();
        
        var app = builder.Build();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();
        app.Run();
    }
}