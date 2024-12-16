using ReactiveUI;

namespace Maui_sample
{
    class MainPageViewModel : ReactiveObject
    {
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

    public bool AppID
    {
      get => _appID;
      set
      {
        if (_appID != value)
        {
          _appID = value;
          Preferences.Default.Set("UseAppID", _appID);
          UpdateAppID();
          this.RaisePropertyChanged();
        }
      }
    }
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
