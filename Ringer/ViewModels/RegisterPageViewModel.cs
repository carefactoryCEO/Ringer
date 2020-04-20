using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Ringer.Core.Data;
using Ringer.Core.Models;
using Ringer.Extensions;
using Ringer.Helpers;
using Ringer.Services;
using Ringer.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    public class RegisterPageViewModel : INotifyPropertyChanged
    {
        private DateTime _birthDate;
        private GenderType _sex;

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
            ToggleAgreeAllCommand = new Command(() => ToggleAgreeAll());
            ToggleAgreeCommand = new Command<Term>(term => ToggleAgree(term));
            ShowTermDetailsCommand = new Command<Term>(async term => await ShowTermDetails(term));
            NextCommand = new Command(async () => await Next());

            TermsList = new ObservableCollection<Term>
            {
                new Term{Title="서비스 동의", Required= true, DetailUrl="https://agiwana.azurewebsites.net/Article/Details/84"},
                new Term{Title="위치 정보 동의", Required = true, DetailUrl="https://agiwana.azurewebsites.net/Article/Details/85"},
                new Term{Title="맞춤형 서비스 안내 동의(선택)", DetailUrl="https://agiwana.azurewebsites.net/Article/Details/86"},
                new Term{Title="마케팅 정보 수신 동의(선택)", DetailUrl="https://agiwana.azurewebsites.net/Article/Details/87"}
            };
        }

        public ICommand ValidateCommand { get; private set; }
        public ICommand ToggleAgreeAllCommand { get; private set; }
        public ICommand ToggleAgreeCommand { get; private set; }
        public ICommand ShowTermDetailsCommand { get; private set; }
        public ICommand NextCommand { get; private set; }

        private Task Next()
        {
            var UnAgreedTerms = TermsList.Where(t => t.Required && !t.Agreed);

            if (UnAgreedTerms.Any())
            {
                foreach (var term in UnAgreedTerms)
                {
                    Shell.Current.DisplayAlert(null, $"{term.Title}는 필수 사항입니다.", "확인");
                }

                return Task.CompletedTask;
            }
            else
            {
                return Shell.Current.GoToAsync($"//{nameof(MapPage)}/{nameof(ChatPage)}");
            }
        }

        public ICommand NextCommand { get; private set; }

        private async Task Next()
        {
            var UnAgreedTerms = TermsList.Where(t => t.Required && !t.Agreed);

            if (UnAgreedTerms.Any())
            {
                foreach (var term in UnAgreedTerms)
                {
                    await Shell.Current.DisplayAlert(null, $"{term.Title}는 필수 사항입니다.", "확인");
                }
            }
            else
            {
                var rest = DependencyService.Get<IRESTService>();

                // register
                var user = new User
                {
                    Name = Name,
                    UserType = UserType.Consumer,
                    BirthDate = _birthDate,
                    Gender = _sex,
                    Email = Email,
                    Password = Password,
                };

                var device = new Core.Models.Device
                {
                    Id = App.DeviceId,
                    DeviceType = Utility.iOS ? Core.Data.DeviceType.iOS : Core.Data.DeviceType.Android,
                    IsOn = true
                };

                if (await rest.RegisterConsumerAsync(user, device))
                {
                    // init messaging
                    var messaging = DependencyService.Get<IMessaging>();

                    await messaging.InitAsync(Constants.HubUrl, App.Token);
                    // go to chatpage
                    await Shell.Current.GoToAsync($"//{nameof(MapPage)}/{nameof(ChatPage)}");
                }
                else
                {
                    await Shell.Current.DisplayAlert(null, $"회원등록에 실패했습니다. 다시 시도해보세요.", "확인");
                }
            }
        }

        private Task ShowTermDetails(Term term)
        {
            var launchOptions = new BrowserLaunchOptions
            {
                Flags = BrowserLaunchFlags.PresentAsFormSheet | BrowserLaunchFlags.LaunchAdjacent,
                //Flags = BrowserLaunchFlags.PresentAsFormSheet,
                TitleMode = BrowserTitleMode.Show,
                LaunchMode = BrowserLaunchMode.SystemPreferred
            };
            return Browser.OpenAsync(term.DetailUrl, launchOptions);
        }

        private void ToggleAgree(Term selectedTerm)
        {
            selectedTerm.Agreed = !selectedTerm.Agreed;
            AllAgreed = TermsList.All(t => t.Agreed);
        }

        private void ToggleAgreeAll()
        {
            var agreed = !AllAgreed;

            foreach (var term in TermsList)
            {
                term.Agreed = agreed;
            }

            AllAgreed = agreed;
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

        public bool AllAgreed { get; set; }

        public ObservableCollection<Term> TermsList { get; set; }
    }

    public class Term : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public bool Agreed { get; set; }
        public bool Required { get; set; }
        public string DetailUrl { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
