using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui_sample
{
    public static class PageHelpers
    {
    public static void LinkEntries(IList<Entry> entryList, bool selectAll = true)
  {
    Entry previousEntry = null;
    foreach (Entry entry in entryList.Reverse())
    {
      if (previousEntry == null)
      {
        entry.ReturnType = ReturnType.Done;
      }
      else
      {
        entry.ReturnType = ReturnType.Next;
        // make new variable so the lamda references the correct object
        Entry nextEntry = previousEntry;
        entry.ReturnCommand = new Command(() =>
        {
          if (selectAll && !string.IsNullOrEmpty(nextEntry.Text))
          {
            nextEntry.CursorPosition = 0;
            nextEntry.SelectionLength = nextEntry?.Text.Length ?? 0;
          }
          nextEntry.Focus();
        });
      }
      previousEntry = entry;
    }
  }
    }
}
