namespace WebApp.ImageManager;

public class VirtualDirectory
{
    public string Name { get; set; }
    public List<VirtualDirectory> Children { get; set; } = new();
    public List<VirtualFile> Files { get; set; } = new();
}