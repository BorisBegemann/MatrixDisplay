using Microsoft.AspNetCore.SignalR;

namespace WebApp.Hubs;

public class DisplayManagerHub : Hub<IDisplayManagerHub>
{
    public static string Path => "/display-manager";
    public async Task DisplayedImageHasChanged(Guid name)
    {
        await Clients.All.DisplayedImageHasChanged(name);
    }
}

public interface IDisplayManagerHub
{
    Task DisplayedImageHasChanged(Guid path);
}