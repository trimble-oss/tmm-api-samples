using ReactiveUI;

namespace Maui_sample
{
    class MainPageViewModel : ReactiveObject
    {
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
