using System;
using Ringer.Core.Models;

namespace Ringer.Models
{
    public class ConsulateModel : Consulate
    {
        public bool IsHeader { get; set; }
        public bool IsFooter { get; set; }
        public bool IsInKorea { get; set; }
    }
}
