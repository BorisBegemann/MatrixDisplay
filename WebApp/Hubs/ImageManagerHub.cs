using Microsoft.AspNetCore.SignalR;

namespace WebApp.Hubs;

public class ImageManagerHub : Hub<IImageManagerHub>
{
    public static string Path => "/image-manager";
    public async Task DirectoryHasChanged(string path)
    {
        await Clients.All.DirectoryHasChanged(path);
    }
    
    public async Task ImageAdded(string path, Guid name)
    {
        await Clients.All.ImageAdded(path, name);
    }
}

public interface IImageManagerHub
{
    Task DirectoryHasChanged(string path);
    Task ImageAdded(string path, Guid name);
}