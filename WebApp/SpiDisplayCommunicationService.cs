using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Spi;
using System.Diagnostics;

namespace WebApp;

public class SpiDisplayCommunicationService : IDisplayCommunicationService
{
    private readonly ILogger<SpiDisplayCommunicationService> _logger;
    private SpiDevice? _spiFront;
    private SpiDevice? _spiBack;
    private PwmChannel _brightnessFront;
    private PwmChannel _brightnessBack;
    private byte _frameBufferIndex = 0;

    private readonly bool _frontIsInverted = true;
    private readonly bool _backIsInverted = true;
    private readonly int _spiFrequency = 1000000;

    public SpiDisplayCommunicationService(ILogger<SpiDisplayCommunicationService> logger)
    {
        _logger = logger;
        InitializeSpi();
        InitializePwm();
    }

    private void InitializePwm()
    {
        _brightnessBack = PwmChannel.Create(0, 0, 500, 0.5D);
        _brightnessBack.Start();
        _brightnessFront = PwmChannel.Create(0, 1, 500, 0.5D);
        _brightnessFront.Start();
    }

    private void InitializeSpi()
    {
        _spiFront?.Dispose();
        _spiFront = SpiDevice.Create(new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = _spiFrequency,
            Mode = SpiMode.Mode2,
            DataFlow = DataFlow.MsbFirst,
            DataBitLength = 8
        });
    
        _spiBack?.Dispose();
        _spiBack = SpiDevice.Create(new SpiConnectionSettings(0, 1)
        {
            ClockFrequency = _spiFrequency,
            Mode = SpiMode.Mode2,
            DataFlow = DataFlow.MsbFirst,
            DataBitLength = 8
        });
    }

    public void SendImage(DisplayImage image)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        
        var frontPayload = _frontIsInverted
            ? image.GetInvertedPayload(_frameBufferIndex)
            : image.GetPayload(_frameBufferIndex);
        
        _logger.LogInformation($"Sending {frontPayload.Length} Bytes to Front Display");

        foreach (var chunk in frontPayload.Chunk(1740))
        {
            _spiFront.Write(chunk);
        }

        _spiFront.Read(new Span<byte>(new byte[20000]));
        
        //foreach (var chunk in frontPayload.Chunk(1740))
        //{
        //    _spiBack.Write(chunk);
        //}
        //_ = _spiBack.ReadByte();
        
        _frameBufferIndex ^= 1;
        stopWatch.Stop();
        _logger.LogInformation("Transfer took {1} ms", stopWatch.ElapsedMilliseconds);
    }
}