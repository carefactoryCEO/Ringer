using System;
using Ringer.Core.Models;

namespace Ringer.Core.Data
{
    public class RoomInformation
    {
        public Room Room { get; set; }
        public Message LastMessage { get; set; }
    }
}
