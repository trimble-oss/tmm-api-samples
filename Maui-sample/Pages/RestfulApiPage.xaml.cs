using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Maui_sample.AccessCode;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Maui_sample;

public partial class RestfulApiPage : ContentPage
{
  private readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

  private RestfulApiViewModel VM => BindingContext as RestfulApiViewModel;

  private HttpClient _client;
  private HttpClient Client => _client ??= new HttpClient { BaseAddress = new Uri("http://localhost:9637/"), Timeout = DefaultTimeout };
  public RestfulApiPage()
	{
		InitializeComponent();
    //BindingContext = this;
    Unloaded += (s, e) =>
    {
      _client?.Dispose();
      _client = null;
    };
  }

  private async Task DisplayAlertOnMainThreadAsync(string title, string message, string cancel)
  {
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
      await DisplayAlert(title, message, cancel);
    });
  }
  private async Task<string> GetResponseTextAsync(HttpResponseMessage response)
  {
    return $"Status: {(int)response.StatusCode}\nReason: {response.ReasonPhrase}\n" + await response.Content.ReadAsStringAsync();
  }

  private void SetAuthorizationHeader()
  {
    string accessCode = AccessCodeManager.Instance.GetNextAccessCode();
    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", accessCode);
  }

  private async Task<IDictionary<string, string>> PromptAsync(string title, string buttonText, IList<(string name, string initialValue, string placeholder)> parameters)
  {
    MultiPromptPage page = new(title, buttonText, parameters);
    await Navigation.PushAsync(page);
    return await page.Task;
  }

  private int? ExtractPortFromResponse(string responseText)
  {
    try
    {
      var jsonStartIndex = responseText.IndexOf('{');
      var jsonString = responseText.Substring(jsonStartIndex);
      var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonString);
      var port = jsonObject["port"]?.Value<int>();
      return port;
    }
    catch (Exception ex)
    {
      Debug.WriteLine("Error extracting port from response: " + ex.Message);
      return null;
    }
  }

  private void Get_AntennaInfo_Button(object sender, EventArgs e)
  {

  }

  private void Set_AntennaInfo_Button(object sender, EventArgs e)
  {

  }

  private async void Get_Correction_Source_Info(object sender, EventArgs e)
  {
    try
    {
      VM.AreButtonsEnabled = false;

      SetAuthorizationHeader();

      var response = await Client.GetAsync("api/v1/correctionSource").ConfigureAwait(false);
      if (response.IsSuccessStatusCode)
      {
        var payload = await response.Content.ReadAsStringAsync();
        payload = JToken.Parse(payload).ToString(Newtonsoft.Json.Formatting.Indented);
        await DisplayAlertOnMainThreadAsync("GET api/v1/correctionSource", payload, "OK");
      }
      else
      {
        await DisplayAlertOnMainThreadAsync("GET api/v1/correctionSource", $"Error: {response.StatusCode}", "OK");
        return;
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
    finally
    {
      VM.AreButtonsEnabled = true;
    }
  }

  private void Set_Correction_Source_Info(object sender, EventArgs e)
  {

  }

  private async void Get_Position_Stream_Info(object sender, EventArgs e)
  {
    try
    {
      VM.AreButtonsEnabled = false;

      List<(string, string, string)> parameters;

#if WINDOWS
      parameters = new List<(string, string, string)>
        {
            new("format", "", "locationV2"),
        };
#else
      parameters = new List<(string, string, string)>
        {
            new("format", "", "locationV1, locationV2"),
        };
#endif

      parameters = LoadPromptParameters("GetPositionStream", parameters);
      var values = await PromptAsync("GET api/v1/positionStream", "Send", parameters);
      if (values == null)
        return;

      SavePromptParameters("GetPositionStream", values);

      SetAuthorizationHeader();

      string url = $"api/v1/positionStream?format={values["format"]}";
      var response = await Client.GetAsync(url).ConfigureAwait(false);

      if (response.IsSuccessStatusCode)
      {
        var responseText = await GetResponseTextAsync(response);
        await DisplayAlertOnMainThreadAsync("Put api/v1/positionStream", responseText, "OK");

        var port = ExtractPortFromResponse(responseText);
        if (port.HasValue)
        {
          LocationWebsocketClientV2.LocationV2Port = port.Value;

          await Shell.Current.GoToAsync(nameof(LocationWebsocketPageV2));
        }
        else
        {
          await DisplayAlertOnMainThreadAsync("Error", "Failed to extract port from response.", "OK");
        }
      }
      else
      {
        await DisplayAlertOnMainThreadAsync("Put api/v1/positionStream", await GetResponseTextAsync(response), "NOPE");
        Debug.WriteLine("Put api/v1/positionStream -- NOPE");
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
    finally
    {
      VM.AreButtonsEnabled = true;
    }
  }

  private string GetParamName(string prefix, string name)
  {
    var paramName = $"{prefix}{name}";
    paramName = paramName.Replace(" ", "_");
    return paramName;
  }
  private List<(string name, string initialValue, string placeholder)> LoadPromptParameters(string prefix, IList<(string name, string initialValue, string placeholder)> parameters)
  {
    var newParameters = new List<(string name, string initialValue, string placeholder)>();
    foreach (var param in parameters)
    {
      string savedValue = Preferences.Default.Get(GetParamName(prefix, param.name), param.initialValue);
      newParameters.Add((param.name, savedValue, param.placeholder));
    }
    return newParameters;
  }
  private void SavePromptParameters(string prefix, IDictionary<string, string> parameters)
  {
    foreach (var key in parameters.Keys)
    {
      Preferences.Default.Set(GetParamName(prefix, key), parameters[key]);
    }
  }

  private async void Get_Reciever_Info(object sender, EventArgs e)
  {
    try
    {
      VM.AreButtonsEnabled = false;

      SetAuthorizationHeader();

      var response = await Client.GetAsync("api/v1/receiver").ConfigureAwait(false);
      if (response.IsSuccessStatusCode)
      {
        var payload = await response.Content.ReadAsStringAsync();
        payload = JToken.Parse(payload).ToString(Newtonsoft.Json.Formatting.Indented);
        await DisplayAlertOnMainThreadAsync("GET api/v1/receiver", payload, "OK");
      }
      else
      {
        await DisplayAlertOnMainThreadAsync("GET api/v1/receiver", $"Error: {response.StatusCode}", "OK");
        return;
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
    finally
    {
      VM.AreButtonsEnabled = true;
    }
  }

  private void Set_Receiver_Connection(object sender, EventArgs e)
  {

  }

  private async void Get_TMM_Info(object sender, EventArgs e)
  {
    try
    {
      VM.AreButtonsEnabled = false;

      SetAuthorizationHeader();

      var response = await Client.GetAsync("api/v1/tmmInfo").ConfigureAwait(false);
      if (response.IsSuccessStatusCode)
      {
        var payload = await response.Content.ReadAsStringAsync();
        payload = JToken.Parse(payload).ToString(Newtonsoft.Json.Formatting.Indented);
        await DisplayAlertOnMainThreadAsync("GET api/v1/tmmInfo", payload, "OK");
      }
      else
      {
        await DisplayAlertOnMainThreadAsync("GET api/v1/tmmInfo", $"Error: {response.StatusCode}", "OK");
        return;
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
    finally
    {
      VM.AreButtonsEnabled = true;
    }
  }

  private async void Send_Ping(object sender, EventArgs e)
  {
    try
    {
      VM.AreButtonsEnabled = false;

      SetAuthorizationHeader();

      var response = await Client.GetAsync("api/v1/test/ping").ConfigureAwait(false);
      if (response.IsSuccessStatusCode)
      {
        var payload = await response.Content.ReadAsStringAsync();
        payload = JToken.Parse(payload).ToString(Newtonsoft.Json.Formatting.Indented);
        await DisplayAlertOnMainThreadAsync("GET api/v1/test/ping", payload, "OK");
        Debug.WriteLine("GET api/v1/test/ping -- OK");
      }
      else
      {
        await DisplayAlertOnMainThreadAsync("GET api/v1/test/ping", await GetResponseTextAsync(response), "NOPE");
        Debug.WriteLine("GET api/v1/test/ping -- NOPE");
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex.Message);
    }
    finally
    {
      VM.AreButtonsEnabled = true;
    }
  }
}
