using System;
using System.ComponentModel;
using System.Windows.Input;

using Xamarin.Forms;

namespace Ringer.ViewModels
{
    public class AboutViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }

        public AboutViewModel()
        {
            Title = "About";

            //OpenWebCommand = new Command(() => Shell.Current.Navigation.PopModalAsync());
            //OpenWebCommand = new Command(() => Device.OpenUri(new Uri("https://xamarin.com/platform")));
        }

        public ICommand OpenWebCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}