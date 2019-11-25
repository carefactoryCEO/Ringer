using System;
using Xamarin.Essentials;

namespace Ringer.Helpers
{
    public static class Settings
    {
        public readonly static string System = "system";
        public readonly static string HubUrl = "https://ringerhub.azurewebsites.net/hubs/chat";
        public readonly static string TokenUrl = "https://ringerhub.azurewebsites.net/auth/login";

        public static string DeviceId
        {
            get => Preferences.Get(nameof(DeviceId), null);
            set => Preferences.Set(nameof(DeviceId), value);
        }

        public static string Token
        {
            get => Preferences.Get(nameof(Token), null);
            set => Preferences.Set(nameof(Token), value);
        }

        public static bool IsLoggedIn => Token != null;

        public static string Name
        {
            get => Preferences.Get(nameof(Name), null);
            set => Preferences.Set(nameof(Name), value);
        }
    }
}
