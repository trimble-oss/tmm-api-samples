namespace Maui_sample;

public partial class LocationWebsocketPageV2 : ContentPage
{
  private readonly LocationWebsocketViewModelV2 _viewModel;
  public LocationWebsocketPageV2(int port)
  {
    InitializeComponent();
    _viewModel = new LocationWebsocketViewModelV2(port);
    BindingContext = _viewModel;
  }

  protected override void OnAppearing()
  {
    try
    {
      base.OnAppearing();
      Task.Run(_viewModel.ConnectAsync);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occurred: {ex.Message}");
    }
  }

  protected override void OnDisappearing()
  {
    base.OnDisappearing();
    _viewModel.Disconnect();
  }
}
