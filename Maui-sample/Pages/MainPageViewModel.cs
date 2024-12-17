using System.Reactive;
using ReactiveUI;

namespace Maui_sample
{
    class MainPageViewModel : ReactiveObject
    {

    private bool _areLabelsVisible;
    public bool AreLabelsVisible
    {
      get => _areLabelsVisible;
      set => this.RaiseAndSetIfChanged(ref _areLabelsVisible, value);
    }

    private string _messages;
    public string Messages
    {
      get => _messages;
      set => this.RaiseAndSetIfChanged(ref _messages, value);
    }

    private double? _latitude;
    public double? Latitude
    {
      get => _latitude;
      set => this.RaiseAndSetIfChanged(ref _latitude, value);
    }

    private double? _longitude;
    public double? Longitude
    {
      get => _longitude;
      set => this.RaiseAndSetIfChanged(ref _longitude, value);
    }

    private double? _altitude;
    public double? Altitude
    {
      get => _altitude;
      set => this.RaiseAndSetIfChanged(ref _altitude, value);
    }
    private bool _appID = false;

    private string _applicationID = string.Empty;

    public string ApplicationID
    {
      get => _applicationID;
      set
      {
        if (_applicationID != value)
        {
          _applicationID = value;
          Preferences.Default.Set("ApplicationID", value);
          UpdateAppID();
          this.RaisePropertyChanged();
        }
      }
    }

    public MainPageViewModel()
    {
      _applicationID = Preferences.Default.Get("ApplicationID", string.Empty);
      _appID = Preferences.Default.Get("UseAppID", false);

    }
    private void UpdateAppID()
    {
      Values.AppID = _appID ? _applicationID.Trim() : Environment.GetEnvironmentVariable("SampleAppID");
    }
  }
}
