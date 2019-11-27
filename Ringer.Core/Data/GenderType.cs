using System.ComponentModel.DataAnnotations;

namespace Ringer.Core.Data
{
    public enum GenderType
    {
        [Display(Name = "여자")]
        Female,
        [Display(Name = "남자")]
        Male
    }
}
