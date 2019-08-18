using System;
using System.Collections.Generic;
using System.Text;

namespace Ringer.Models
{
    public class Infomation
    {
        public string Title { get; set; }

        public string Location { get; set; }

        public string PhoneNumber { get; set; }

        public string Extras { get; set; }

        public string DisplayLocation => "주소: " + Location;

        public string DisplayPhoneNumber => string.IsNullOrEmpty(PhoneNumber) ? null : "전화번호: " + PhoneNumber;
    }
}
