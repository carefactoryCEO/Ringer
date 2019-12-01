using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ringer.Core.Data;

namespace Ringer.Core.Models
{
    public class User
    {
        public int Id { get; set; }

        [Display(Name = "이름")]
        public string Name { get; set; }

        [Display(Name = "구분")]
        public UserType UserType { get; set; }

        [Display(Name = "생년월일")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; }

        [Display(Name = "성별")]
        public GenderType Gender { get; set; }

        [Display(Name = "휴대폰번호")]
        public string PhoneNumber { get; set; }

        [Display(Name = "이메일")]
        public string Email { get; set; }

        [Display(Name = "비밀번호")]
        public string Password { get; set; }

        public DateTime CreatedAt { get; set; }

        [InverseProperty("Owner")]
        public ICollection<Device> Devices { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
