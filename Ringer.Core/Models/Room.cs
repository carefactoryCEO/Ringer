using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ringer.Core.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsClosed { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}