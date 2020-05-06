using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ringer.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Ringer.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            NameLabel.Text = App.UserName;
            EmailLabel.Text = App.Email;
            VersionLabel.Text = Constants.VersionString;
        }

        async void ShowTermsGeneral(object sender, EventArgs e)
        {
            await Browser.OpenAsync(Constants.BaseUrl + "/terms/general", BrowserLaunchMode.SystemPreferred);
        }

        async void ShowTermsLocation(object sender, EventArgs e)
        {
            await Browser.OpenAsync(Constants.BaseUrl + "/terms/location", BrowserLaunchMode.SystemPreferred);

        }

        async void ShowTermsPrivate(object sender, EventArgs e)
        {
            await Browser.OpenAsync(Constants.BaseUrl + "/terms/private", BrowserLaunchMode.SystemPreferred);
        }

        async void Help(object sender, EventArgs e)
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = "문의드립니다",
                    Body = null,
                    To = new List<string> { Constants.HelpMail }
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException fbsEx)
            {
                // Email is not supported on this device
                Debug.WriteLine(fbsEx);
            }
            catch (Exception ex)
            {
                // Some other exception occurred
                Debug.WriteLine(ex);
            }
        }
    }
}
