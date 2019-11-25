using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ringer.Core.Models
{
    public class User
    {
        public int ID { get; set; }

        [Display(Name = "이름")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "생년월일")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; }

        [Display(Name = "성별")]
        public GenderType Gender { get; set; }

        [Display(Name = "전화번호")]
        [Required]
        public string PhoneNumber { get; set; }

        [Display(Name = "이메일")]
        public string Email { get; set; }

        [Display(Name = "비밀번호")]
        public string Password { get; set; }

        public ICollection<Ticket> Tickets { get; set; }

    }

    public enum GenderType
    {
        [Display(Name = "여자")]
        Female,
        [Display(Name = "남자")]
        Male
    }
}
