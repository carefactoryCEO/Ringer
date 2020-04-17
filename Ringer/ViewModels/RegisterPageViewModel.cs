using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Ringer.Core.Data;
using Ringer.Extensions;
using Ringer.Helpers;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    public class RegisterPageViewModel : INotifyPropertyChanged
    {
        private DateTime _birthDate;
        private GenderType _sex;

        public string Title { get; set; } = "이름을 입력해주세요.";

        public string Name { get; set; }
        public string BirthDate { get; set; }
        public string Sex { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public bool IsContinueButtonEnabled =>
            !string.IsNullOrWhiteSpace(Name) &&
            !string.IsNullOrWhiteSpace(BirthDate) &&
            !string.IsNullOrWhiteSpace(Sex) &&
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Password);

        public ICommand ValidateCommand { get; set; }

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
            ValidateCommand = new Command(async () => await Validate());
        }

        private async Task Validate()
        {
            if (await ValidateNameAsync()
                && await ValidateBirthDateAndSexAsync()
                && await ValidateEmailAsync()
                && await ValidatePasswordAsync()
                )
            {
                MessagingCenter.Send(this, "ShowTermsView");
            }
        }

        private async Task<bool> ValidatePasswordAsync()
        {
            if (!Regex.IsMatch(Password, @"^[~`!@#$%\^&*()-+=a-zA-Z0-9]{6,20}$"))
            {
                await Shell.Current.DisplayAlert(null, "비밀번호는 영문 대소문자, 숫자, 특수문자를 조합해 6자이상 20자 이하 길이여야 합니다.", "확인");
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateEmailAsync()
        {
            if (!Regex.IsMatch(Email, @"^[a-z0-9_+.-\.]+@([a-z0-9-]+\.)+[a-zA-Z0-9]{2,4}$"))
            {
                await Shell.Current.DisplayAlert(null, "입력한 이메일은 정확한 형식이 아닙니다.", "확인");
                return false;
            }
            return true;
        }

        private async Task<bool> ValidateBirthDateAndSexAsync()
        {
            if (!Regex.IsMatch(BirthDate, @"^\d{6}$"))
            {
                await Shell.Current.DisplayAlert(null, "주민등록번호 앞 6자리는 생년월일입니다.\n예를들어 1998년 3월 12일은 980312와 같이 쓰세요.", "확인");
                return false;
            }

            if (!Regex.IsMatch(Sex, @"^\d$"))
            {
                await Shell.Current.DisplayAlert(null, "성별은 1자리 숫자로 써야합니다.\n아래와 같이 쓰세요.\n\n2000년 이전에 태어난\n남자는 1, 여자는 2\n\n2000년 이후 태어난\n남자는 3, 여자는 4", "확인");
                return false;
            }

            var sevenString = BirthDate + Sex;

            if (!sevenString.IsValidBirthDateAndSex(out var birthDate, out var sex))
            {
                await Shell.Current.DisplayAlert(null, "입력한 주민등록번호는 정확한 형식이 아닙니다.", "확인");
                return false;
            }

            _birthDate = birthDate;
            _sex = sex;

            return true;
        }

        private async Task<bool> ValidateNameAsync()
        {
            if (Regex.IsMatch(Name, @"[a-zA-Z]"))
            {
                // Gildong Hong
                if (!Regex.IsMatch(Name, @"^[a-zA-Z]+\s[a-zA-Z]+$"))
                {
                    await Shell.Current.DisplayAlert(null, "영문 이름은 성과 이름을 공백으로 구분해야 합니다.\n예를 들면 Gildong Hong과 같이 써주세요.", "확인");
                    return false;
                }
            }
            else if (Name?.Length < 3 || !Regex.IsMatch(Name, @"^[가-힣]+$"))
            {
                await Shell.Current.DisplayAlert(null, "이름은 공백 없이 성과 이름을 한글로 써야합니다.", "확인");
                return false;
            }

            return true;
        }
    }

}
