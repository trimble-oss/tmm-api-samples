namespace Maui_sample;

public partial class LocationWebsocketPage : ContentPage
{
  private readonly LocationWebsocketViewModel _viewModel;
  public LocationWebsocketPage(int port)
	{
		InitializeComponent();
    _viewModel = new LocationWebsocketViewModel(port);
    BindingContext = _viewModel;
  }
  protected override void OnAppearing()
  {
    base.OnAppearing();

    _viewModel.Connect();
  }

  protected override void OnDisappearing()
  {
    base.OnDisappearing();

    _viewModel.Disconnect();
  }
}
