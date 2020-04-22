using System;
using System.Collections.Generic;

namespace Ringer.Core.Models
{
    public enum TermsType
    {
        SeviceGeneral,
        Location,
        Marketing,
        CustomizedInformation
    }

    public class Terms
    {
        public int Id { get; set; }
        public bool IsCurrent { get; set; }
        public bool Required { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Version { get; set; }
        public TermsType Type { get; set; }
        public DateTime CreaetedAt { get; set; }

        public ICollection<Agreement> Agreements { get; set; }
    }

    public class Agreement
    {
        public int Id { get; set; }
        public DateTime AgreedAt { get; set; }

        public int TermsId { get; set; }
        public Terms Terms { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }


}
