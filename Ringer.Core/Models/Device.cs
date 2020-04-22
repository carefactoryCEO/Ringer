using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Ringer.Core.Data;

namespace Ringer.Core.Models
{
    public class Device
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public DeviceType DeviceType { get; set; }
        public bool IsOn { get; set; }
        public bool IsActive { get; set; }
        public string ConnectionId { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }
    }
}