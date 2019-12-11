using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ringer.Core.Models;
using Xamarin.Forms;

namespace RingerStaff.ViewModels
{
    public class UsersActivePageViewModel : BaseViewModel
    {
        public UsersActivePageViewModel()
        {
            users = new ObservableCollection<User>
            {
                new User { Name = "신모범", Email = "mbshin@carefactory.co.kr" },
                new User { Name = "김순용", Email = "sykim@carefactory.co.kr"}
            };

            AddUserCommand = new Command(() => Users.Add(new User { Name = "김은미", Email = "dejavent@gmail.com" }));
        }

        private ObservableCollection<User> users;
        public ObservableCollection<User> Users
        {
            get => users;
            set => SetProperty(ref users, value);
        }

        public ICommand AddUserCommand { get; set; }
    }
}
