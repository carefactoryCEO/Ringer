using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    public class RegisterPageViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; } = "이름을 입력해주세요.";

        private string name;
        private string birthDate;
        private string sex;
        private string email;
        private string password;

        public string Name
        {
            get => name;
            set
            {
                if (name == value)
                    return;

                name = value;

                IsConfirmButtonVisible = name.Length > 0;

                OnPropertyChanged();
            }
        }

        public string BirthDate
        {
            get => birthDate;
            set
            {
                if (birthDate == value)
                    return;

                birthDate = value;

                OnPropertyChanged();

                if (birthDate.Length == 6)
                    MessagingCenter.Send(this, "FocusEntry", "SexEntry");
            }
        }

        public string Sex
        {
            get => sex;
            set
            {
                if (sex == value)
                    return;

                sex = value;

                OnPropertyChanged();

                if (sex.Length > 0)
                    ConfirmCommand.Execute(null);
            }
        }

        public string Email
        {
            get => email;
            set
            {
                if (email == value)
                    return;

                email = value;

                IsConfirmButtonVisible = email.Length > 0;

                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                if (password == value)
                    return;

                password = value;

                IsConfirmButtonVisible = password.Length > 7;

                OnPropertyChanged();
            }
        }

        public bool IsConfirmButtonVisible { get; set; } = false;

        public bool IsNextButtonVisible { get; set; } = false;

        public bool IsBirthDateVisible { get; set; }

        public bool IsEmailVisible { get; set; }

        public bool IsPasswordVisible { get; set; }

        public ICommand ConfirmCommand { get; set; }

        public ICommand NextCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RegisterPageViewModel()
        {
            ConfirmCommand = new Command(() => Confirm());
        }

        private void Confirm()
        {
            if (!IsBirthDateVisible)
            {
                IsBirthDateVisible = true;
                IsConfirmButtonVisible = false;

                MessagingCenter.Send(this, "FocusEntry", "BirthDateEntry");

                Title = "주민등록번호를 입력해주세요.";

                return;
            }

            if (!IsEmailVisible)
            {
                IsEmailVisible = true;
                IsConfirmButtonVisible = false;

                MessagingCenter.Send(this, "FocusEntry", "EmailEntry");

                Title = "이메일을 입력해주세요.";

                return;
            }

            if (!IsPasswordVisible)
            {
                IsPasswordVisible = true;
                IsConfirmButtonVisible = false;

                MessagingCenter.Send(this, "FocusEntry", "PasswordEntry");

                Title = "비밀번호를 입력해주세요.";

                return;
            }

            IsNextButtonVisible = true;
            IsConfirmButtonVisible = false;

            MessagingCenter.Send(this, "UnFocusEntry", "PasswordEntry");

            Title = "입력한 내용을 확인해주세요.";
        }
    }

}
