using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.AppCenter.Analytics;
using Ringer.Core.Models;
using Ringer.Types;
using Xamarin.Forms;

namespace Ringer.Helpers
{
    public class Utility
    {
        public static bool IsChatActive => App.IsChatPage && App.IsOn;
        public static bool iOS => Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS;
        public static bool Android => Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android;
        public static bool AndroidCameraActivated => Android && App.IsCameraActivated;
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
        public static void Trace(object obj = default, bool analyticsAlso = false, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "")
        {
            string message = obj.ToString();

            message = $"[{DateTime.UtcNow.ToString("MMddHHmmss")}]({Path.GetFileNameWithoutExtension(callerFilePath)}.{callerName}){message}";

            Debug.WriteLine(message);

            if (analyticsAlso)
                Analytics.TrackEvent(message);
        }

        internal static void Trace(Consulate currentSelection)
        {
            throw new NotImplementedException();
        }
    }
}
