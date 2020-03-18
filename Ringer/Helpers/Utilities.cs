using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.AppCenter.Analytics;
using Ringer.Types;

namespace Ringer.Helpers
{
    public class Utilities
    {
        public static bool InSameMinute(DateTime current, DateTime last)
        {
            return current - last < TimeSpan.FromMinutes(1) && current.Minute == last.Minute;
        }

        public static MessageTypes GetMediaAndDirectionType(string body, int senderId, int userId)
        {
            // set default
            MessageTypes messageTypes = MessageTypes.Text | MessageTypes.Leading | MessageTypes.Trailing;

            // set media type
            if (body != null)
            {
                string videoPattern = @"^https://ringerstoragekr.blob.core.windows.net/ringer/[\w-]+(?:\.mov|\.mp4)$";
                string imagePattern = @"^https://ringerstoragekr.blob.core.windows.net/ringer/[\w-]+\.jpg$";

                if (Regex.IsMatch(body, videoPattern))
                {
                    messageTypes |= MessageTypes.Video;
                    messageTypes ^= MessageTypes.Text;
                }

                if (Regex.IsMatch(body, imagePattern))
                {
                    messageTypes |= MessageTypes.Image;
                    messageTypes ^= MessageTypes.Text;
                }
            }

            // set direction
            messageTypes |= (senderId == userId) ? MessageTypes.Outgoing : MessageTypes.Incomming;

            return messageTypes;
        }


        public static void Trace(string message = "", bool analyticsAlso = false, [CallerMemberName] string callerName = "")
        {
            message = $"\n[{DateTime.UtcNow.ToString("yy-MM-dd HH:mm:ss")}]{callerName}: {message}";

            Debug.WriteLine(message);

            if (analyticsAlso)
                Analytics.TrackEvent(message);
        }
    }
}
