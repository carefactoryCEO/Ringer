using System;
using System.Threading.Tasks;

namespace Ringer.Services
{
    public interface INotificationPermissionService
    {
        Task<bool> IsNotificationPermissionGranted();
        Task<bool> RequestNotificationPermission();
    }
}
