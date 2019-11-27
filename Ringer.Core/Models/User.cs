using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        public ICollection<MobileDevice> Devices { get; set; }

        public ICollection<ChattingRoom> Rooms { get; set; }

        // TODO: migration으로 전환하고 IsConnected, ConnectionID, DeviceID 등 필요한 property를 추가한다.
        //public bool IsConnected { get; set; }
    }

    public class ChattingRoom
    {
        public int ID { get; set; }

        public string Name { get; set; }

        //public ICollection<User> Users { get; set; }

        public ICollection<MobileDevice> Devices { get; set; }
    }

    public class MobileDevice
    {
        public string ID { get; set; }

        public string Connection { get; set; }

        [Required]
        public OSType OS { get; set; }

        [Required]
        public User Owner { get; set; }

        public bool IsConnected => Connection != null;
    }

    public class Message
    {
        public Message() { }

        public Message(string content, string sender = "system") : this()
        {
            Content = content;
            Sender = sender;
        }

        public string Content { get; set; }
        public string Sender { get; set; } // sender
    }

    public enum OSType
    {
        iOS,
        Android
    }
}
