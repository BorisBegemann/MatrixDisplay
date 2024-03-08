using SixLabors.ImageSharp;

namespace WebApp;

public class DisplayImage
{
    private readonly Image _image;
    
    public DisplayImage(Image image)
    {
        if (image == null)
        {
            throw new ArgumentNullException(nameof(image));
        }
        
        if (image.Size.Width != 580 || image.Size.Height != 104)
        {
            throw new ArgumentException("Image must be 580x104 pixels", nameof(image));
        }
        
        _image = image;
    }
    
    public IEnumerable<byte> GetPayload()
    {
        using var ms = new MemoryStream();
        _image.SaveAsPng(ms);
        return ms.ToArray();
    }
}