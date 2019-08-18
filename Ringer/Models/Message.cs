using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Ringer.Models
{
    public class Message
    {
        public string Text { get; set; }
        public string User { get; set; }
        public Color Color { get; internal set; }
    }
}
