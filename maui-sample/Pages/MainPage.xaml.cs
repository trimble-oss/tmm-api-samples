namespace MauiSample;

public partial class MainPage : ContentPage
{
  public MainPage()
  {
    PlatformRequestService.Instance.Initialize();
    InitializeComponent();
  }
}
