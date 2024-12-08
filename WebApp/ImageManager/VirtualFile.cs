namespace WebApp.ImageManager;

public class VirtualFile(string name, DateTime createdAt, DateTime? lastModified, string createdBy)
{
    public static VirtualFile New(string name, string? createdBy) => new(name, DateTime.Now, null, createdBy ?? "Unknown");

    public string CreatedBy { get; } = createdBy;
    public DateTime CreatedAt { get; } = createdAt;
    public DateTime? LastModified { get; set; } = lastModified;
    public string Name { get; } = name;
}