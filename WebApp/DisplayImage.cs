using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WebApp;

public class DisplayImage
{
    private readonly Image<Rgb24> _image;
    
    public DisplayImage(Image<Rgb24> image)
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
        _image.ProcessPixelRows(accessor =>
        {
            for (var verticalIndex = 0; verticalIndex < _image.Height; verticalIndex++)
            {
                var row = accessor.GetRowSpan(verticalIndex);
                var px = row[0].ToScaledVector4().Length() > 0.5f;
            }
        });
        using var ms = new MemoryStream();
        _image.SaveAsPng(ms);
        return ms.ToArray();
    }
}