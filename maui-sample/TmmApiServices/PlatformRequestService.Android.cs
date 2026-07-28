using Android.Content;
using Android.OS;
using MauiSample.Models;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;

namespace MauiSample;

public partial class PlatformRequestService
{
  internal class CustomActivityResultCallback : Java.Lang.Object, IActivityResultCallback
  {
    private readonly Action<ActivityResult> _action;
    public CustomActivityResultCallback(Action<ActivityResult> action)
    {
      _action = action;
    }
    public void OnActivityResult(Java.Lang.Object? result)
    {
      if (result is not null)
      {
        _action?.Invoke((ActivityResult)result);
      }
    }
  }

  private ActivityResultLauncher? _activityResultLauncher;
  private MainActivity? _mainActivity;
  private TaskCompletionSource<ActivityResult>? _taskCompletionSource;

  partial void InitializePlatform()
  {
    _mainActivity = Platform.CurrentActivity as MainActivity;
    _activityResultLauncher = _mainActivity?.RegisterForActivityResult(
        new ActivityResultContracts.StartActivityForResult(),
        new CustomActivityResultCallback((result) => _taskCompletionSource?.SetResult(result)));
  }

  public partial async Task<RegistrationDetails?> RegisterAsync(string applicationID)
  {
    try
    {
      Intent intent = new("com.trimble.tmm.REGISTER");
      intent.PutExtra("applicationID", applicationID);
      ActivityResult? result = await LaunchActivityForResultAsync(intent);
      if (result is not null && result.ResultCode == (int)Android.App.Result.Ok)
      {
        return GetResultFromBundle(result.Data?.Extras);
      }
      return null;
    }
    catch
    {
      return null;
    }
  }

  private static RegistrationDetails GetResultFromBundle(Bundle? bundle)
  {
    var keyset = bundle?.KeySet();
    if (keyset is null)
    {
      throw new ArgumentNullException("KeySet cannot be null");
    }
    RegistrationDetails result = new RegistrationDetails
    {
      RegistrationResult = bundle?.GetString("registrationResult") ?? string.Empty,
      LocationPort = bundle?.GetInt("locationPort", 0) ?? 0,
      LocationSecurePort = bundle?.GetInt("locationSecurePort", 0) ?? 0,
      ApiPort = bundle?.GetInt("apiPort", 0) ?? 0,
      ApiSecurePort = bundle?.GetInt("apiSecurePort", 0) ?? 0,
      LocationV2Port = bundle?.GetInt("locationV2Port", 0) ?? 0,
      LocationV2SecurePort = bundle?.GetInt("locationV2SecurePort", 0) ?? 0,
    };
    return result;
  }

  public partial Task ShowReceiverSelectionAsync()
  {
    try
    {
      Intent intent = new("com.trimble.tmm.RECEIVERSELECTION");
      _mainActivity?.StartActivity(intent);
    }
    catch
    {
    }

    return Task.CompletedTask;
  }

  private async Task<ActivityResult?> LaunchActivityForResultAsync(Intent intent)
  {
    _taskCompletionSource = new();
    _activityResultLauncher?.Launch(intent);
    return await _taskCompletionSource.Task;
  }
}
