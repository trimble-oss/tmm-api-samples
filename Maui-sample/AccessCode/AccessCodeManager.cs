using System;
using System.Collections.Generic;
using System.Linq;
using Maui_sample.Clock;
using Maui_sample.AccessCode;

namespace Maui_sample
{
    class AccessCodeManager
    {
    private DateTime _lastResetTime;
    private readonly IClock _clock;

    public static AccessCodeManager Instance { get; } = new AccessCodeManager();

    private AccessCodeManager() : this(new SystemClock())
    {
    }

    public AccessCodeManager(IClock clock)
    {
      _clock = clock;
      _lastResetTime = DateTime.MinValue;
    }

    public string GetNextAccessCode()
    {
      DateTime time = GetCurrentTimeRounded();
      if (time > _lastResetTime)

        _lastResetTime = time;

      AccessCodeGenerator acg = new(Values.AppID, time);
      return acg.AccessCode;
    }

    private DateTime GetCurrentTimeRounded()
    {
      DateTime currentTime = _clock.UtcNow;
      return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour,
          currentTime.Minute, currentTime.Second, DateTimeKind.Utc);
    }
  }
}
