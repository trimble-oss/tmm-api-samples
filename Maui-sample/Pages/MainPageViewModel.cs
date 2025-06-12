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

    private string _receiverName;
    public string ReceiverName
    {
      get => _receiverName;
      set => this.RaiseAndSetIfChanged(ref _receiverName, value);
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

    private string _applicationID = string.Empty;

    public string ApplicationID
    {
      get => _applicationID;
      set
      {
        if (_applicationID != value)
        {
          _applicationID = value;
          Values.AppID = value;
          this.RaisePropertyChanged();
        }
      }
    }

    private string _registrationStatus;
    public string RegistrationStatus
    {
      get => _registrationStatus;
      set => this.RaiseAndSetIfChanged(ref _registrationStatus, value);
    }


    public MainPageViewModel()
    {
      _messages = string.Empty;
      _applicationID = Values.AppID;
      _receiverName = string.Empty;
      _registrationStatus = string.Empty;
    }
  }
}
