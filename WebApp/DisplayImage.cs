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

    private static readonly int[] SwathAddresses = new[] { 7, 3, 5, 1, 6, 2, 4, 0 };
    private static readonly Dictionary<int, int> AddressOffsets = new()
    {
        { 2, 0 }, 
        { 4, 7 }, 
        { 7, 14 }, 
        { 3, 21 }, 
        { 5, 28 }, 
        { 1, 34 }, 
        { 0, 40 }, 
        { 6, 46 }
    };
    
    private static readonly Dictionary<int, byte> FourthByteByAddress = new()
    {
        { 0, 0b00000000 },
        { 1, 0b00000000 },
        { 2, 0b11111111 },
        { 3, 0b11111111 },
        { 4, 0b11111111 },
        { 5, 0b00000000 },
        { 6, 0b00000000 },
        { 7, 0b11111111 }
    };
    
    private static readonly Dictionary<int, bool> IsVerticallyInverted = new()
    {
        { 2, false },
        { 7, false },
        { 5, false },
        { 0, false },
        { 4, true },
        { 3, true },
        { 1, true },
        { 6, true }
    };
    
    private static readonly Dictionary<(int Address, int Index), bool> IsInverted = new()
    {
        { (2,1), false },
        { (7,1), false },
        { (5,1), false },
        { (0,1), false },
        { (4,1), false },
        { (3,1), false },
        { (1,1), false },
        { (6,1), false },
        { (7,0), true },
        { (2,0), true },
        { (5,0), true },
        { (0,0), true },
        { (4,0), true },
        { (3,0), true },
        { (1,0), true },
        { (6,0), true },
    };
    
    private static readonly Dictionary<int, int> SwathHeightByAddress = new() { { 2, 7 }, { 7, 7 }, { 5, 6 }, { 0, 6 }, { 4, 7 }, { 3, 7 }, { 1, 6 }, { 6, 6 } };
    
    public IEnumerable<byte> GetPayload(byte frameBufferIndex)
    {
        if (frameBufferIndex > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(frameBufferIndex));
        }
        
        var serializedImage = new List<byte>();
        _image.ProcessPixelRows(accessor =>
        {
            var buffer = new bool[7, 580];
            foreach (var swathAddress in SwathAddresses)
            {
                for (var swathIndex = 0; swathIndex < 2; swathIndex++)
                {
                    if (IsInverted[(swathAddress, swathIndex)])
                    {
                        InvertAndCopyPixelDataToBuffer(accessor, swathAddress, ref buffer);
                    }
                    else
                    {
                        CopyPixelDataToBuffer(accessor, swathAddress, ref buffer);
                    }
                    
                    SerializeBuffer(swathAddress, frameBufferIndex, ref buffer, ref serializedImage);
                }
            }
        });
        return serializedImage;
    }

    private static void CopyPixelDataToBuffer(PixelAccessor<Rgb24> accessor, int swathAddress, ref bool[,] buffer)
    {
        var verticalOffset = AddressOffsets[swathAddress];
        var height = SwathHeightByAddress[swathAddress];
        
        for (var intraSwathVerticalIndex = 0; intraSwathVerticalIndex < height; intraSwathVerticalIndex++)
        {
            var row = accessor.GetRowSpan(verticalOffset + intraSwathVerticalIndex);
            for (var intraSwathHorizontalIndex = 0; intraSwathHorizontalIndex < 580; intraSwathHorizontalIndex++)
            {
                if (IsVerticallyInverted[swathAddress])
                {
                    buffer[height - 1 - intraSwathVerticalIndex, intraSwathHorizontalIndex] =  row[intraSwathHorizontalIndex] is { R: > 128, G: > 128, B: > 128 };
                }
                else
                {
                    buffer[intraSwathVerticalIndex, intraSwathHorizontalIndex] =  row[intraSwathHorizontalIndex] is { R: > 128, G: > 128, B: > 128 };
                }
            }
        }
    }
    
    private static void InvertAndCopyPixelDataToBuffer(PixelAccessor<Rgb24> accessor, int swathAddress, ref bool[,] buffer)
    {
        var verticalOffset = AddressOffsets[swathAddress];
        var height = SwathHeightByAddress[swathAddress];
        
        for (var intraSwathVerticalIndex = 0; intraSwathVerticalIndex < height; intraSwathVerticalIndex++)
        {
            var row = accessor.GetRowSpan(103 - verticalOffset - intraSwathVerticalIndex);
            for (var intraSwathHorizontalIndex = 0; intraSwathHorizontalIndex < 580; intraSwathHorizontalIndex++)
            {
                if (IsVerticallyInverted[swathAddress])
                {
                    buffer[height - 1 - intraSwathVerticalIndex, intraSwathHorizontalIndex] = row[579 - intraSwathHorizontalIndex] is { R: > 128, G: > 128, B: > 128 };
                }
                else
                {
                    buffer[intraSwathVerticalIndex, intraSwathHorizontalIndex] = row[579 - intraSwathHorizontalIndex] is { R: > 128, G: > 128, B: > 128 };
                }
                
            }
        }
    }

    private static void SerializeBuffer(int swathAddress, byte frameBufferIndex, ref bool[,] buffer, ref List<byte> serializedData)
    {
        var height = SwathHeightByAddress[swathAddress];
        var serializedImage = new List<byte>();
        for (var horizontalIndex = 0; horizontalIndex < 580; horizontalIndex += 4)
        {
            var data = new byte[4] { 0, 0, 0, 0 };
            data[0] |= buffer[0, horizontalIndex + 0] ? (byte)0b11111000 : (byte)0b00000000;
            data[1] |= buffer[0, horizontalIndex + 1] ? (byte)0b00010000 : (byte)0b00000000;
            data[2] |= buffer[0, horizontalIndex + 2] ? (byte)0b00100000 : (byte)0b00000000;
            data[3] |= buffer[0, horizontalIndex + 3] ? (byte)0b01000000 : (byte)0b00000000;
            
            data[0] |= buffer[1, horizontalIndex + 0] ? (byte)0b00000100 : (byte)0b00000000;
            data[1] |= buffer[1, horizontalIndex + 1] ? (byte)0b00001000 : (byte)0b00000000;
            data[2] |= buffer[1, horizontalIndex + 2] ? (byte)0b00010000 : (byte)0b00000000;
            data[3] |= buffer[1, horizontalIndex + 3] ? (byte)0b00100000 : (byte)0b00000000;
            
            data[0] |= buffer[2, horizontalIndex + 0] ? (byte)0b00000010 : (byte)0b00000000;
            data[1] |= buffer[2, horizontalIndex + 1] ? (byte)0b00000100 : (byte)0b00000000;
            data[2] |= buffer[2, horizontalIndex + 2] ? (byte)0b00001000 : (byte)0b00000000;
            data[3] |= buffer[2, horizontalIndex + 3] ? (byte)0b00010000 : (byte)0b00000000;
            
            data[0] |= buffer[3, horizontalIndex + 0] ? (byte)0b00000001 : (byte)0b00000000;
            data[1] |= buffer[3, horizontalIndex + 1] ? (byte)0b00000010 : (byte)0b00000000;
            data[2] |= buffer[3, horizontalIndex + 2] ? (byte)0b00000100 : (byte)0b00000000;
            data[3] |= buffer[3, horizontalIndex + 3] ? (byte)0b00001000 : (byte)0b00000000;
            
            data[1] |= buffer[4, horizontalIndex + 0] ? (byte)0b10000000 : (byte)0b00000000;
            data[1] |= buffer[4, horizontalIndex + 1] ? (byte)0b00000001 : (byte)0b00000000;
            data[2] |= buffer[4, horizontalIndex + 2] ? (byte)0b00000010 : (byte)0b00000000;
            data[3] |= buffer[4, horizontalIndex + 3] ? (byte)0b00000100 : (byte)0b00000000;
            
            data[1] |= buffer[5, horizontalIndex + 0] ? (byte)0b01000000 : (byte)0b00000000;
            data[2] |= buffer[5, horizontalIndex + 1] ? (byte)0b10000000 : (byte)0b00000000;
            data[2] |= buffer[5, horizontalIndex + 2] ? (byte)0b00000001 : (byte)0b00000000;
            data[3] |= buffer[5, horizontalIndex + 3] ? (byte)0b00000010 : (byte)0b00000000;

            if (height > 6)
            {
                data[1] |= buffer[6, horizontalIndex + 0] ? (byte)0b00100000 : (byte)0b00000000;
                data[2] |= buffer[6, horizontalIndex + 1] ? (byte)0b01000000 : (byte)0b00000000;
                data[3] |= buffer[6, horizontalIndex + 2] ? (byte)0b10000000 : (byte)0b00000000;
                data[3] |= buffer[6, horizontalIndex + 3] ? (byte)0b00000001 : (byte)0b00000000;
            }

            serializedData.AddRange(data);
            serializedData.Add(FourthByteByAddress[swathAddress]);
            var addressByte = (byte)(swathAddress << 3);
            addressByte |= (byte)(frameBufferIndex << 2);
            addressByte += 3;
            
            serializedData.Add(addressByte);
        }
        
        serializedData[^2] |= 0b00000000;
    }
}