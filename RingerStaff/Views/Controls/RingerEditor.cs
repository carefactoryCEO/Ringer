using System;
using Xamarin.Forms;

namespace RingerStaff.Views.Controls
{
    public class RingerEditor : Editor
    {
        public static BindableProperty CornerRadiusProperty
            = BindableProperty.Create(nameof(CornerRadius), typeof(float), typeof(RingerEditor), 16f);

        public float CornerRadius
        {
            get => (float)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #region Placeholder
        public new static BindableProperty PlaceholderProperty
            = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(RingerEditor));

        public new string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }
        #endregion

        #region Placeholder color
        public new static BindableProperty PlaceholderColorProperty
            = BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(RingerEditor), Color.LightGray);

        public new Color PlaceholderColor
        {
            get => (Color)GetValue(PlaceholderColorProperty);
            set => SetValue(PlaceholderColorProperty, value);
        }
        #endregion

        #region Has Rounded Corner
        public static BindableProperty HasRoundedCornerProperty
            = BindableProperty.Create(nameof(HasRoundedCorner), typeof(bool), typeof(RingerEditor), false);

        public bool HasRoundedCorner
        {
            get => (bool)GetValue(HasRoundedCornerProperty);
            set => SetValue(HasRoundedCornerProperty, value);
        }
        #endregion

        #region Is Expandable
        public static BindableProperty IsExpandableProperty
            = BindableProperty.Create(nameof(IsExpandable), typeof(bool), typeof(RingerEditor), false);

        public bool IsExpandable
        {
            get => (bool)GetValue(IsExpandableProperty);
            set => SetValue(IsExpandableProperty, value);
        }
        #endregion

        public RingerEditor()
        {
            TextChanged += RingerEditor_TextChanged;
        }

        ~RingerEditor()
        {
            TextChanged -= RingerEditor_TextChanged;
        }

        private void RingerEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsExpandable)
                InvalidateMeasure();
        }
    }
}
