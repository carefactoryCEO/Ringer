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
        public string Name { get; set; }
        public string BirthDate { get; set; }
        public string Sex { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        //public bool IsConfirmButtonVisible { get; set; } = false;

        //public bool IsNextButtonVisible { get; set; } = false;

        //public bool IsBirthDateVisible { get; set; }

        //public bool IsEmailVisible { get; set; }

        //public bool IsPasswordVisible { get; set; }

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
            //if (!IsBirthDateVisible)
            //{
            //    IsBirthDateVisible = true;
            //    IsConfirmButtonVisible = false;

            //    MessagingCenter.Send(this, "FocusEntry", "BirthDateEntry");

            //    Title = "주민등록번호를 입력해주세요.";

            //    return;
            //}

            //if (!IsEmailVisible)
            //{
            //    IsEmailVisible = true;
            //    IsConfirmButtonVisible = false;

            //    MessagingCenter.Send(this, "FocusEntry", "EmailEntry");

            //    Title = "이메일을 입력해주세요.";

            //    return;
            //}

            //if (!IsPasswordVisible)
            //{
            //    IsPasswordVisible = true;
            //    IsConfirmButtonVisible = false;

            //    MessagingCenter.Send(this, "FocusEntry", "PasswordEntry");

            //    Title = "비밀번호를 입력해주세요.";

            //    return;
            //}

            //IsNextButtonVisible = true;
            //IsConfirmButtonVisible = false;

            //MessagingCenter.Send(this, "UnFocusEntry", "PasswordEntry");

            //Title = "입력한 내용을 확인해주세요.";
        }
    }

}
