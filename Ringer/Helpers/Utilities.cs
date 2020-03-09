using System;
namespace Ringer.Helpers
{
    public class Utilities
    {
        public static bool InSameMinute(DateTime current, DateTime last)
        {
            return current - last < TimeSpan.FromMinutes(1) && current.Minute == last.Minute;
        }
    }
}
