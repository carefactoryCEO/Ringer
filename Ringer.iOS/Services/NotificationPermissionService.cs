using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Ringer.Services;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

[assembly: Dependency(typeof(Ringer.iOS.Services.NotificationPermissionService))]
namespace Ringer.iOS.Services
{
    public class NotificationPermissionService : INotificationPermissionService
    {
        private static bool? _alertsAllowed;

        public async Task<bool> IsNotificationPermissionGranted()
        {
            if (_alertsAllowed.HasValue)
            {
                return _alertsAllowed.Value;
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0) == false)
            {
                _alertsAllowed = true;
                return true;
            }

            var settings = await UNUserNotificationCenter.Current.GetNotificationSettingsAsync();
            var allowed = settings.AlertSetting == UNNotificationSetting.Enabled;

            if (allowed)
            {
                _alertsAllowed = true;
                return true;
            }

            _alertsAllowed = false;
            return false;
        }

        public async Task<bool> RequestNotificationPermission()
        {
            var (alertsAllowed, error) = await UNUserNotificationCenter.Current.RequestAuthorizationAsync(
                        UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound);

            _alertsAllowed = alertsAllowed;
            Debug.WriteLine(error?.LocalizedDescription);

            return _alertsAllowed.Value;
        }
    }
}
