using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

[BindProperties]
public class Configuration : PageModel
{
    private readonly IDisplayCommunicationService _displayCommunicationService;

    public Configuration(IDisplayCommunicationService displayCommunicationService)
    {
        _displayCommunicationService = displayCommunicationService;
        
        SpiClockFrequency = _displayCommunicationService.SpiClockFrequency;
        FrontLatchPin = _displayCommunicationService.FrontLatchPin;
        BackLatchPin = _displayCommunicationService.BackLatchPin;
        SendToFront = _displayCommunicationService.SendToFront;
        SendToBack = _displayCommunicationService.SendToBack;
        InvertBack = _displayCommunicationService.InvertBack;
    }
    
    public int SpiClockFrequency { get; set; }
    public int FrontLatchPin { get; set; }
    public int BackLatchPin { get; set; }
    public bool SendToFront { get; set; }
    public bool SendToBack { get; set; }
    public bool InvertBack { get; set; }
    
    public void OnPost()
    {
        _displayCommunicationService.SpiClockFrequency = SpiClockFrequency;
        _displayCommunicationService.FrontLatchPin = FrontLatchPin;
        _displayCommunicationService.BackLatchPin = BackLatchPin;
        _displayCommunicationService.SendToFront = SendToFront;
        _displayCommunicationService.SendToBack = SendToBack;
        _displayCommunicationService.InvertBack = InvertBack;
        _displayCommunicationService.RestartComm();
    }
}