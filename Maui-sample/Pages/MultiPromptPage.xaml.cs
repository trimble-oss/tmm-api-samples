namespace Maui_sample;

public partial class MultiPromptPage : ContentPage
{
  private readonly TaskCompletionSource<IDictionary<string, string>> _tcs = new();

  private MultiPromptViewModel _vm;
  private int _count;

  public Task<IDictionary<string, string>> Task => _tcs.Task;

  public MultiPromptPage(string title, string buttonText, IList<(string name, string initialValue, string placeholder)> parameters)
  {
    InitializeComponent();
    _vm.SetParameters(parameters);
    _vm.Title = title;
    _vm.ButtonText = buttonText;
    _count = parameters.Count;
    LinkEntries();
  }

  protected override void OnBindingContextChanged()
  {
    base.OnBindingContextChanged();
    _vm = (MultiPromptViewModel)BindingContext;
  }

  private void LinkEntries()
  {
    List<Entry> entries = new()
    {
      Item0Value,
      Item1Value,
      Item2Value,
      Item3Value,
      Item4Value,
      Item5Value,
      Item6Value,
      Item7Value,
      Item8Value,
      Item9Value
    };
    PageHelpers.LinkEntries(entries.Take(_count).ToList(), true);
  }

  protected override void OnDisappearing()
  {
    base.OnDisappearing();
    if (!_tcs.Task.IsCompleted)
      _tcs.SetResult(null);
  }

  private async void Button_Clicked(object sender, EventArgs e)
  {
    try
    {
      _tcs.SetResult(_vm.Values);
      await Navigation.PopAsync();
    }
    catch
    {
    }
  }
}
