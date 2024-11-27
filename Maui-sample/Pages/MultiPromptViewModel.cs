using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Maui_sample
{
    class MultiPromptViewModel : ReactiveObject
    {
      private struct Item
      {
        public string Name;
        public string Value;
        public string Placeholder;
        public Item(string name, string value, string placeholder)
        {
          Name = name;
          Value = value;
          Placeholder = placeholder;
        }
      }

      public const int MAX_ITEMS = 10;

      private Item[] _items = Array.Empty<Item>();

      public IDictionary<string, string> Values => _items.ToDictionary(item => item.Name, item => item.Value);

    public void SetParameters(IList<(string name, string initialValue, string placeholder)> parameters)
    {
      if (parameters.Count <= 0 || parameters.Count > MAX_ITEMS)
      {
        throw new ArgumentException($"Supports 1 - {MAX_ITEMS} items");
      }

      _items = new Item[parameters.Count];
      for (int i = 0; i < parameters.Count; i++)
      {
        _items[i] = new Item(parameters[i].name, parameters[i].initialValue, parameters[i].placeholder);
      }
      this.RaisePropertyChanged("");
    }

    [Reactive]
    public string Title { get; set; } = "MultiPrompt";

    [Reactive]
    public string ButtonText { get; set; } = "OK";

    public bool IsItem0Visible => GetIsItemVisible(0);
    public bool IsItem1Visible => GetIsItemVisible(1);
    public bool IsItem2Visible => GetIsItemVisible(2);
    public bool IsItem3Visible => GetIsItemVisible(3);
    public bool IsItem4Visible => GetIsItemVisible(4);
    public bool IsItem5Visible => GetIsItemVisible(5);
    public bool IsItem6Visible => GetIsItemVisible(6);
    public bool IsItem7Visible => GetIsItemVisible(7);
    public bool IsItem8Visible => GetIsItemVisible(8);
    public bool IsItem9Visible => GetIsItemVisible(9);

    public string Item0Name => GetItemName(0);
    public string Item1Name => GetItemName(1);
    public string Item2Name => GetItemName(2);
    public string Item3Name => GetItemName(3);
    public string Item4Name => GetItemName(4);
    public string Item5Name => GetItemName(5);
    public string Item6Name => GetItemName(6);
    public string Item7Name => GetItemName(7);
    public string Item8Name => GetItemName(8);
    public string Item9Name => GetItemName(9);

    public string Item0Placeholder => GetItemPlaceholder(0);
    public string Item1Placeholder => GetItemPlaceholder(1);
    public string Item2Placeholder => GetItemPlaceholder(2);
    public string Item3Placeholder => GetItemPlaceholder(3);
    public string Item4Placeholder => GetItemPlaceholder(4);
    public string Item5Placeholder => GetItemPlaceholder(5);
    public string Item6Placeholder => GetItemPlaceholder(6);
    public string Item7Placeholder => GetItemPlaceholder(7);
    public string Item8Placeholder => GetItemPlaceholder(8);
    public string Item9Placeholder => GetItemPlaceholder(9);

    public string Item0Value
    {
      get => GetItemValue(0);
      set
      {
        if (IsItem0Visible)
          this.RaiseAndSetIfChanged(ref _items[0].Value, value);
      }
    }
    public string Item1Value
    {
      get => GetItemValue(1);
      set
      {
        if (IsItem1Visible)
          this.RaiseAndSetIfChanged(ref _items[1].Value, value);
      }
    }
    public string Item2Value
    {
      get => GetItemValue(2);
      set
      {
        if (IsItem2Visible)
          this.RaiseAndSetIfChanged(ref _items[2].Value, value);
      }
    }
    public string Item3Value
    {
      get => GetItemValue(3);
      set
      {
        if (IsItem3Visible)
          this.RaiseAndSetIfChanged(ref _items[3].Value, value);
      }
    }
    public string Item4Value
    {
      get => GetItemValue(4);
      set
      {
        if (IsItem4Visible)
          this.RaiseAndSetIfChanged(ref _items[4].Value, value);
      }
    }
    public string Item5Value
    {
      get => GetItemValue(5);
      set
      {
        if (IsItem5Visible)
          this.RaiseAndSetIfChanged(ref _items[5].Value, value);
      }
    }
    public string Item6Value
    {
      get => GetItemValue(6);
      set
      {
        if (IsItem6Visible)
          this.RaiseAndSetIfChanged(ref _items[6].Value, value);
      }
    }
    public string Item7Value
    {
      get => GetItemValue(7);
      set
      {
        if (IsItem7Visible)
          this.RaiseAndSetIfChanged(ref _items[7].Value, value);
      }
    }
    public string Item8Value
    {
      get => GetItemValue(8);
      set
      {
        if (IsItem8Visible)
          this.RaiseAndSetIfChanged(ref _items[8].Value, value);
      }
    }
    public string Item9Value
    {
      get => GetItemValue(9);
      set
      {
        if (IsItem9Visible)
          this.RaiseAndSetIfChanged(ref _items[9].Value, value);
      }
    }

    private bool GetIsItemVisible(int i) => i >= 0 && i < _items.Length;

    private string GetItemName(int i)
    {
      if (GetIsItemVisible(i))
        return _items[i].Name;
      else
        return "";
    }

    private string GetItemPlaceholder(int i)
    {
      if (GetIsItemVisible(i))
        return _items[i].Placeholder;
      else
        return "";
    }

    private string GetItemValue(int i)
    {
      if (GetIsItemVisible(i))
        return _items[i].Value;
      else
        return "";
    }
  }
}
