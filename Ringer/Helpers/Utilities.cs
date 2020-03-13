using System;
using System.Text.RegularExpressions;
using Ringer.Types;

namespace Ringer.Helpers
{
    public class Utilities
    {
        public static bool InSameMinute(DateTime current, DateTime last)
        {
            return current - last < TimeSpan.FromMinutes(1) && current.Minute == last.Minute;
        }

        public static MessageTypes SetMessageTypes(string body, int senderId, int userId)
        {
            MessageTypes messageTypes = MessageTypes.Text | MessageTypes.Leading | MessageTypes.Trailing;

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

            messageTypes |= (senderId == userId) ? MessageTypes.Outgoing : MessageTypes.Incomming;

            return messageTypes;
        }
    }
}
