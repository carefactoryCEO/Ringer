﻿using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Ringer.iOS.Renderers;
using Ringer.Views.Controls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ExtendedEditorControl), typeof(CustomEditorRenderer))]
namespace Ringer.iOS.Renderers
{
    public class CustomEditorRenderer : EditorRenderer
    {
        UILabel _placeholderLabel;
        double previousHeight = -1;
        int prevLines = 2;
        nfloat _cornerRadius = 16;

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            // Control == UITextView
            if (Control != null)
            {
                Control.TextContainer.LineBreakMode = UILineBreakMode.CharacterWrap;

                if (_placeholderLabel == null)
                {
                    CreatePlaceholder();
                }
            }

            if (e.NewElement != null)
            {
                var customControl = (ExtendedEditorControl)e.NewElement;

                if (customControl.IsExpandable)
                    Control.ScrollEnabled = false;
                else
                    Control.ScrollEnabled = true;

                if (customControl.HasRoundedCorner)
                {
                    Control.Layer.CornerRadius = _cornerRadius;
                    Control.Layer.BorderColor = Color.LightGray.ToCGColor();
                    Control.Layer.BorderWidth = 1;

                    Control.TextContainerInset = new UIEdgeInsets(
                        Control.TextContainerInset.Top,
                        Control.TextContainerInset.Left + 6,
                        Control.TextContainerInset.Bottom,
                        Control.TextContainerInset.Right + 33);
                }
                else
                    Control.Layer.CornerRadius = 0;

                Control.InputAccessoryView = new UIView(CGRect.Empty);

            }

            if (e.OldElement != null)
            {

            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var customControl = (ExtendedEditorControl)Element;

            if (e.PropertyName == Editor.TextProperty.PropertyName)
            {
                if (customControl.IsExpandable)
                {
                    CGSize size = Control.Text.StringSize(Control.Font, Control.TextContainer.Size, UILineBreakMode.CharacterWrap);

                    int numLines = (int)(size.Height / Control.Font.LineHeight);

                    if (prevLines > numLines)
                    {
                        customControl.HeightRequest = -1;

                    }
                    else if (string.IsNullOrEmpty(Control.Text))
                    {
                        customControl.HeightRequest = -1;
                    }

                    prevLines = numLines;
                }

                _placeholderLabel.Hidden = !string.IsNullOrEmpty(Control.Text);

            }
            else if (e.PropertyName == VisualElement.HeightProperty.PropertyName)
            {
                if (customControl.IsExpandable)
                {
                    CGSize size = Control.Text.StringSize(Control.Font, Control.TextContainer.Size, UILineBreakMode.CharacterWrap);

                    int numLines = (int)(size.Height / Control.Font.LineHeight);

                    if (numLines > 4)
                    {
                        Control.ScrollEnabled = true;
                        customControl.HeightRequest = previousHeight;
                    }
                    else
                    {
                        Control.ScrollEnabled = false;
                        previousHeight = customControl.Height;

                    }
                }
            }
            else if (ExtendedEditorControl.PlaceholderProperty.PropertyName == e.PropertyName)
            {
                _placeholderLabel.Text = customControl.Placeholder;
            }
            else if (ExtendedEditorControl.PlaceholderColorProperty.PropertyName == e.PropertyName)
            {
                _placeholderLabel.TextColor = customControl.PlaceholderColor.ToUIColor();
            }
            else if (ExtendedEditorControl.HasRoundedCornerProperty.PropertyName == e.PropertyName)
            {
                if (customControl.HasRoundedCorner)
                {
                    Control.Layer.CornerRadius = _cornerRadius;
                    Control.Layer.BorderColor = Color.Gray.ToCGColor();
                    Control.Layer.BorderWidth = 1;

                    Control.TextContainerInset = new UIEdgeInsets(
                        Control.TextContainerInset.Top,
                        Control.TextContainerInset.Left + 6,
                        Control.TextContainerInset.Bottom,
                        Control.TextContainerInset.Right + 30);
                }
                else
                    Control.Layer.CornerRadius = 0;
            }
            else if (ExtendedEditorControl.IsExpandableProperty.PropertyName == e.PropertyName)
            {
                if (customControl.IsExpandable)
                    Control.ScrollEnabled = false;
                else
                    Control.ScrollEnabled = true;

            }

        }

        public void CreatePlaceholder()
        {
            var element = Element as ExtendedEditorControl;

            _placeholderLabel = new UILabel
            {
                Text = element?.Placeholder,
                TextColor = element.PlaceholderColor.ToUIColor(),
                BackgroundColor = UIColor.Clear
            };

            var edgeInsets = Control.TextContainerInset;
            var lineFragmentPadding = Control.TextContainer.LineFragmentPadding;

            Control.AddSubview(_placeholderLabel);

            var vConstraints = NSLayoutConstraint.FromVisualFormat(
                "V:|-" + edgeInsets.Top + "-[PlaceholderLabel]-" + edgeInsets.Bottom + "-|", 0, new NSDictionary(),
                NSDictionary.FromObjectsAndKeys(
                    new NSObject[] { _placeholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
            );

            var hConstraints = NSLayoutConstraint.FromVisualFormat(
                "H:|-" + lineFragmentPadding + "-[PlaceholderLabel]-" + lineFragmentPadding + "-|",
                0, new NSDictionary(),
                NSDictionary.FromObjectsAndKeys(
                    new NSObject[] { _placeholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
            );

            _placeholderLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            Control.AddConstraints(hConstraints);
            Control.AddConstraints(vConstraints);
        }
    }
}