using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;

namespace WebApp;

public class SpiDisplayCommunicationService : IDisplayCommunicationService
{
    private readonly ILogger<SpiDisplayCommunicationService> _logger;
    private SpiDevice? _spiFront;
    private SpiDevice? _spiBack;
    private GpioPin? _latchPinFront;
    private GpioPin? _latchPinBack;
    private byte _frameBufferIndex = 0;

    private readonly bool _frontIsInverted = true;
    private readonly bool _backIsInverted = true;
    private readonly int _spiFrequency = 1000000;

    public SpiDisplayCommunicationService(ILogger<SpiDisplayCommunicationService> logger)
    {
        _logger = logger;
        InitializeSpi();
    }

    private void InitializeSpi()
    {
        _spiFront?.Dispose();
        _spiFront = SpiDevice.Create(new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = _spiFrequency,
            Mode = SpiMode.Mode2,
            DataBitLength = 8
        });
    
        _spiBack?.Dispose();
        _spiBack = SpiDevice.Create(new SpiConnectionSettings(1, 0)
        {
            ClockFrequency = _spiFrequency,
            Mode = SpiMode.Mode2,
            DataBitLength = 8
        });
    
        _latchPinFront = new GpioController().OpenPin(0, PinMode.Output);
        _latchPinFront.Write(PinValue.Low);
    
        _latchPinBack = new GpioController().OpenPin(1, PinMode.Output);
        _latchPinBack.Write(PinValue.Low);
    }

    public void SendImage(DisplayImage image)
    {
        if (_spiFront == null || _spiBack == null || _latchPinFront == null || _latchPinBack == null)
        {
            _logger.LogWarning("SPI not initialized, skipping image");
            return;
        }
        
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var readBuffer = new Span<byte>(new byte[1740]);
        
        /*
        var backPayload = _backIsInverted
            ? image.GetInvertedPayload(_frameBufferIndex)
            : image.GetPayload(_frameBufferIndex);
        
        _logger.LogInformation($"Sending {backPayload.Length} Bytes to Back Display");

        foreach (var chunk in backPayload.Chunk(1740))
        {
            _spiBack.TransferFullDuplex(chunk, readBuffer);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
            _latchPinBack.Write(PinValue.High);
            Task.Delay(TimeSpan.FromTicks(20)).Wait();
            _latchPinBack.Write(PinValue.Low);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
        }
        */
        
        var frontPayload = _frontIsInverted
            ? image.GetInvertedPayload(_frameBufferIndex)
            : image.GetPayload(_frameBufferIndex);
        
        _logger.LogInformation($"Sending {frontPayload.Length} Bytes to Front Display");

        foreach (var chunk in frontPayload.Chunk(1740))
        {
            _spiFront.TransferFullDuplex(chunk, readBuffer);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
            _latchPinFront.Write(PinValue.High);
            Task.Delay(TimeSpan.FromTicks(20)).Wait();
            _latchPinFront.Write(PinValue.Low);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
        }
        
        foreach (var chunk in frontPayload.Chunk(1740))
        {
            _spiBack.TransferFullDuplex(chunk, readBuffer);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
            _latchPinBack.Write(PinValue.High);
            Task.Delay(TimeSpan.FromTicks(20)).Wait();
            _latchPinBack.Write(PinValue.Low);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
        }
        
        _frameBufferIndex ^= 1;
        stopWatch.Stop();
        _logger.LogInformation("Transfer took {1} ms", stopWatch.ElapsedMilliseconds);
    }
}