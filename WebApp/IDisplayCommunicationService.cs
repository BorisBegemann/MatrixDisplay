namespace WebApp;

public interface IDisplayCommunicationService
{
    public int FrontLatchPin { get; set; }
    public int BackLatchPin  { get; set; }
    public int SpiClockFrequency  { get; set; }
    public bool SendToFront { get; set; }
    public bool SendToBack { get; set; }
    public bool InvertBack { get; set; }
    int LatchDelayInTicks { get; set; }
    int LatchDurationInTicks { get; set; }
    void SendImage(DisplayImage image);
    void RestartComm();
}