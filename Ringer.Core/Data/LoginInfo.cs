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

        public string DeviceType { get; set; }
        public string DeviceId { get; set; }
    }
}
