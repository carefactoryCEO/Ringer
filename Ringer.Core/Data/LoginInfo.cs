using System;

namespace Ringer.Core.Data
{
    public class LoginInfo
    {
        public string LoginType { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public GenderType Gender { get; set; }

        public DeviceType DeviceType { get; set; }
        public string DeviceId { get; set; }
    }

    public class ResponseJson
    {
        public string token { get; set; }
        public string roomId { get; set; }
    }

    public class DeviceReport
    {
        public string DeviceId { get; set; }
        public bool Status { get; set; }
    }
}
