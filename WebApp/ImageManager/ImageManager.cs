using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using WebApp.Hubs;

namespace WebApp.ImageManager;

public class ImageManager
{
    private static readonly SemaphoreSlim Semaphore = new(1);
    private const string StorageFile = "storage.json";
    private readonly VirtualDirectory _root;
    private readonly string _physicalBasePath;
    private readonly IHubContext<ImageManagerHub, IImageManagerHub> _hubContext;

    public ImageManager(IHostEnvironment environment, IHubContext<ImageManagerHub, IImageManagerHub> hubContext)
    {
        _hubContext = hubContext;
        _physicalBasePath = Path.Combine(environment.ContentRootPath, "uploads");

        if (File.Exists(Path.Combine(_physicalBasePath, StorageFile)))
        {
            using var reader = new StreamReader(Path.Combine(_physicalBasePath, StorageFile));
            var json = reader.ReadToEnd();
            _root = JsonSerializer.Deserialize<VirtualDirectory>(json) ?? new VirtualDirectory { Name = "/" };
        }
        else
        {
            _root = new VirtualDirectory { Name = "/" };
            var files = Directory.GetFiles(_physicalBasePath);
            foreach (var file in files)
            {
                try { Image.Load(file); } catch (Exception) { continue; }
                var creationTime = File.GetCreationTime(file);
                
                if (Path.GetExtension(file) != ".png")
                {
                    var filenameWithExtension = Path.ChangeExtension(file, ".png");
                    File.Copy(file, filenameWithExtension);
                    File.Delete(file);
                    _root.Files.Add(new VirtualFile(Path.GetFileName(filenameWithExtension), creationTime,null, "Unknown"));
                }
                else
                {
                    _root.Files.Add(new VirtualFile(Path.GetFileName(file), creationTime,null, "Unknown"));
                }
            }
            
            FlushStorageFile().Wait();
        }
    }
    
    private async Task FlushStorageFile()
    {
        var json = JsonSerializer.Serialize(_root);
        await File.WriteAllTextAsync(Path.Combine(_physicalBasePath, StorageFile), json);
    }

    public IReadOnlyList<string> GetImages(string path = "/")
    {
        var directory = _root;
        if (path != "/")
        {
            var directories = path.Split(Path.PathSeparator);
            foreach (var dir in directories)
            {
                directory = directory.Children.FirstOrDefault(x => x.Name == dir)
                            ?? throw new ArgumentException("Path does not exist", nameof(path));
            }
        }

        return directory.Files.Select(x => x.Name).ToList();
    }
    
    public async Task SaveImage(Image image, string virtualPath = "/", string? userName = null)
    {
        await Semaphore.WaitAsync();
        try
        {
            var name = Guid.NewGuid() + ".png";
            var directory = _root;
            if (virtualPath != "/")
            {
                var directories = virtualPath.Split(Path.PathSeparator);
                foreach (var dir in directories)
                {
                    directory = directory.Children.FirstOrDefault(x => x.Name == dir)
                                ?? throw new ArgumentException("Path does not exist", nameof(virtualPath));
                }
            }
        
            directory.Files.Add(VirtualFile.New(name, userName));
            await image.SaveAsync(Path.Combine(_physicalBasePath, name), new PngEncoder());
            await _hubContext.Clients.All.ImageAdded(virtualPath, name);
            await _hubContext.Clients.All.DirectoryHasChanged(virtualPath);
            await FlushStorageFile();
        }
        finally
        {
            Semaphore.Release();
        }
    }
}