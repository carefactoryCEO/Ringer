using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace RingerStaff.Views
{
    public partial class StaffsPage : ContentPage
    {
        public StaffsPage()
        {
            InitializeComponent();

            BirthDate.TextChanged += BirthDate_TextChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            NumericEntry.Focus();
        }

        private void BirthDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.NewTextValue.Length == 6)
                Gender.Focus();
        }

        string originalText = "⎕ ⎕ ⎕ ⎕ ⎕ ⎕ - ⎕ ●●●●●●";

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var returnText = originalText;

            for (int i = 0; i < e.NewTextValue.Length; i++)
            {
                var currentChar = e.NewTextValue[i];

                if (i > 5)
                    i++;

                returnText = returnText.Remove(i * 2, 1).Insert(i * 2, currentChar.ToString());
            }

            BirthDateLabel.Text = returnText;

        }
    }
}
