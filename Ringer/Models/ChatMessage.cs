using MvvmHelpers;
using System;
using Xamarin.Forms;

namespace Ringer.Models
{
    public class ChatMessage : ObservableObject
    {
        static Random Random = new Random();

        string user;
        string message;
        string firstLetter;
        Color color;
        Color backgroundColor;

        public string User
        {
            get => user;
            set => SetProperty(ref user, value);
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public string FirstLetter
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(firstLetter))
                    return firstLetter;

                firstLetter = User?.Length > 0 ? User[0].ToString() : "?";
                return firstLetter;
            }
            set => firstLetter = value;
        }

        public Color Color
        {
            get
            {
                if (color != null && color.A != 0)
                    return color;

                color = Color.FromRgb(Random.Next(0, 255), Random.Next(0, 255), Random.Next(0, 255)).MultiplyAlpha(.9);
                return color;
            }
            set => color = value;
        }

        public Color BackgroundColor
        {
            get
            {
                if (backgroundColor != null && backgroundColor.A != 0)
                    return backgroundColor;

                backgroundColor = Color.MultiplyAlpha(.6);
                return backgroundColor;
            }
            set => backgroundColor = value;
        }
    }
}
