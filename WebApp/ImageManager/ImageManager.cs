using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using WebApp.Hubs;

namespace WebApp.ImageManager;

public class ImageManager
{
    public static Guid InitImage = Guid.Empty;
    
    private readonly SemaphoreSlim _semaphore = new(1);
    private const string StorageFile = "storage.json";
    private bool _isStorageDirty = false;
    private readonly VirtualDirectory _root;
    private readonly string _imageBasePath;
    private readonly string _specialBasePath;
    private readonly IHubContext<ImageManagerHub, IImageManagerHub> _hubContext;

    private readonly Dictionary<Guid, Image<Rgb24>> _temporaryImages = new();
    
    public ImageManager(IHostEnvironment environment, IHubContext<ImageManagerHub, IImageManagerHub> hubContext)
    {
        _hubContext = hubContext;
        _imageBasePath = Path.Combine(environment.ContentRootPath, "uploads");
        _specialBasePath = Path.Combine(environment.ContentRootPath, "wwwroot", "img");

        if (File.Exists(Path.Combine(_imageBasePath, StorageFile)))
        {
            using var reader = new StreamReader(Path.Combine(_imageBasePath, StorageFile));
            var json = reader.ReadToEnd();
            _root = JsonSerializer.Deserialize<VirtualDirectory>(json) ?? new VirtualDirectory { Name = "/" };
        }
        else
        {
            _root = new VirtualDirectory { Name = "/" };
            var files = Directory.GetFiles(_imageBasePath);
            foreach (var file in files)
            {
                try { Image.Load(file); } catch (Exception) { continue; }
                var creationTime = File.GetCreationTime(file);
                
                if (Path.GetExtension(file) != ".png")
                {
                    if (!Guid.TryParse(file, out var name))
                    {
                        continue;
                    };
                    
                    var filenameWithExtension = Path.ChangeExtension(file, ".png");
                    File.Copy(file, filenameWithExtension);
                    File.Delete(file);
                    _root.Files.Add(new VirtualFile(name, creationTime,null, "Unknown"));
                }
                else
                {
                    if (!Guid.TryParse(Path.GetFileNameWithoutExtension(file), out var name))
                    {
                        continue;
                    };
                    
                    _root.Files.Add(new VirtualFile(name, creationTime,null, "Unknown"));
                }
            }
            
            FlushStorageFile().Wait();
        }
    }
    
    private async Task FlushStorageFile()
    {
        await _semaphore.WaitAsync().ConfigureAwait(true);
        try
        {
            if (_isStorageDirty)
            {
                var json = JsonSerializer.Serialize(_root);
                await File.WriteAllTextAsync(Path.Combine(_imageBasePath, StorageFile), json).ConfigureAwait(true);
            }
        }
        catch (Exception)
        {
            _isStorageDirty = false;
            _semaphore.Release();
        }
        
    }

    public IReadOnlyList<Guid> ListImages(string path = "/")
    {
        var directory = GetDirectoryForPath(path);
        return directory.Files.Select(x => x.Name).ToList();
    }

    private VirtualDirectory GetDirectoryForPath(string path)
    {
        var directory = _root;
        if (path != "/")
        {
            var directories = path[1 ..].Split('/');
            foreach (var dir in directories)
            {
                directory = directory.Children.FirstOrDefault(x => x.Name == dir)
                            ?? throw new ArgumentException("Path does not exist", nameof(path));
            }
        }

        return directory;
    }
    
    private VirtualDirectory GetParentDirectoryForPath(string path)
    {
        var directory = _root;
        if (path != "/")
        {
            var directories = path[1 ..].Split('/');
            foreach (var dir in directories[.. ^1])
            {
                directory = directory.Children.FirstOrDefault(x => x.Name == dir)
                            ?? throw new ArgumentException("Path does not exist", nameof(path));
            }
        }

        return directory;
    }

    public IReadOnlyList<(string Name, string Path, bool IsExpandable)> GetChildren(string path)
    {
        return GetDirectoryForPath(path)
            .Children
            .Select(x => (x.Name, path != "/" ? path + "/" + x.Name : path + x.Name, x.Children.Any()))
            .ToList();
    }

    public bool TryCreateDirectory(string path, string directoryName)
    {
        var parent = GetDirectoryForPath(path);
        if (parent.Children.All(x => x.Name != directoryName) 
            && IsValidDirectoryName(directoryName))
        {
            parent.Children.Add(new VirtualDirectory { Name = directoryName });
            _isStorageDirty = true;
            return true;
        }

        return false;
    }

    private bool IsValidDirectoryName(string directoryName)
    {
        if (directoryName.Contains(Path.DirectorySeparatorChar) || directoryName.Contains(Path.AltDirectorySeparatorChar))
        {
            return false;
        }
        
        return !Guid.TryParse(directoryName, out _);
    }

    public Guid CreateTemporaryImage(Image<Rgb24> image)
    {
        var name = Guid.NewGuid();
        _temporaryImages.Add(name, image);
        return name;
    }

    public Image<Rgb24> GetImage(Guid name)
    {
        if (name == InitImage)
        {
            return Image.Load<Rgb24>(Path.Combine(_specialBasePath, "init.png"));
        }
        
        if (_temporaryImages.TryGetValue(name, out var img))
        {
            return img;
        }

        return Image.Load<Rgb24>(Path.Combine(_imageBasePath, name + ".png"));
    }
    
    public async Task<Guid> SaveImage(Image image, string virtualPath = "/", string? userName = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            var name = Guid.NewGuid();
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
            await image.SaveAsync(Path.Combine(_imageBasePath, name + ".png"), new PngEncoder());
            await _hubContext.Clients.All.ImageAdded(virtualPath, name);
            await _hubContext.Clients.All.DirectoryHasChanged(virtualPath);
            return name;
        }
        finally
        {
            _isStorageDirty = true;
            _semaphore.Release();
        }
    }

    public bool TryRemoveDirectory(string path)
    {
        var directory = GetDirectoryForPath(path);
        var parentDirectory = GetParentDirectoryForPath(path);
        if (directory.Children.Count == 0 && directory.Files.Count == 0 && parentDirectory.Children.Remove(directory))
        {
            _isStorageDirty = true;
            return true;
        }

        return false;
    }

    public async Task MoveImage(Guid image, string sourcePath, string targetPath)
    {
        var sourceDirectory = GetDirectoryForPath(sourcePath);
        var targetDirectory = GetDirectoryForPath(targetPath);
        if (sourceDirectory.Files.Any(x => x.Name == image))
        {
            var file = sourceDirectory.Files.First(x => x.Name == image);
            sourceDirectory.Files.Remove(file);
            targetDirectory.Files.Add(file);
            //await _hubContext.Clients.All.ImageMoved(sourcePath, targetPath, image);
            await _hubContext.Clients.All.DirectoryHasChanged(sourcePath);
            await _hubContext.Clients.All.DirectoryHasChanged(targetPath);
            _isStorageDirty = true;
        }
    }

    public string GetParentPath(string path)
    {
        return path == "/" ? "/" : string.Join('/', path.Split('/')[..^1]);  
    }
}