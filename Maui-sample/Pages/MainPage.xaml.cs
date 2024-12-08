namespace Maui_sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
    Title = $"TMM Test App (v{GetAppVersionString()})";
    // Retrieves the version number and displays as the title
  }

  private string GetAppVersionString()
  {
#if WINDOWS
    var version = Windows.ApplicationModel.Package.Current.Id.Version;
    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
#else
    return "";
#endif
  }
}
