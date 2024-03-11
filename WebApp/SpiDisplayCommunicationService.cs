using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Spi;

namespace WebApp;

public class SpiDisplayCommunicationService : IDisplayCommunicationService
{
    private readonly ILogger<SpiDisplayCommunicationService> _logger;
    private readonly SpiDevice _dataSpi;
    private readonly GpioPin _latchPin;
    private byte _frameBufferIndex = 0;
    private PwmChannel _pwm;
    public SpiDisplayCommunicationService(ILogger<SpiDisplayCommunicationService> logger)
    {
        _logger = logger;
        _dataSpi = SpiDevice.Create(new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = 1000000,
            Mode = SpiMode.Mode2,
            DataBitLength = 8
        });
        _latchPin = new GpioController().OpenPin(0, PinMode.Output);
        _latchPin.Write(PinValue.Low);
        
        _pwm = PwmChannel.Create(0, 0, 500000, 0.5);
        _pwm.Start();
    }

    public void SetPwmDutyCycle(double dutyCycle)
    {
        _pwm.DutyCycle = dutyCycle;
    }
    
    public void SendImage(DisplayImage image)
    {
        var payload = image.GetPayload(_frameBufferIndex).ToArray();
        _logger.LogInformation($"Sending {payload.Length} Bytes to Display");
        
        foreach (var chunk in payload.Chunk(1740))
        {
            _dataSpi.TransferFullDuplex(chunk, new Span<byte>(new byte[chunk.Length]));
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
            _latchPin.Write(PinValue.High);
            Task.Delay(TimeSpan.FromTicks(20)).Wait();
            _latchPin.Write(PinValue.Low);
            Task.Delay(TimeSpan.FromTicks(10)).Wait();
        }
        
        _frameBufferIndex ^= 1;
    }
}