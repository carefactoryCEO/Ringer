using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using RingerStaff.iOS.Renderers;
using RingerStaff.Views.Partials;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ChatInputBarView), typeof(ChatInputBarViewRenderer))]
namespace RingerStaff.iOS.Renderers
{
    public class ChatInputBarViewRenderer : ViewRenderer
    {
        NSObject _keyboardShowObserver;
        NSObject _keyboardHideObserver;
        private Thickness _originalMargin;

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                RegisterForKeyboardNotifications();
            }

            if (e.OldElement != null)
            {
                UnregisterForKeyboardNotifications();
            }
        }

        private void RegisterForKeyboardNotifications()
        {
            if (_keyboardShowObserver == null)
                _keyboardShowObserver = UIKeyboard.Notifications.ObserveWillShow(OnKeyboardShow);

            if (_keyboardHideObserver == null)
                _keyboardHideObserver = UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHide);
        }

        private void OnKeyboardShow(object sender, UIKeyboardEventArgs e)
        {
            NSValue result = (NSValue)e.Notification.UserInfo.ObjectForKey(new NSString(UIKeyboard.FrameEndUserInfoKey));
            CGSize keyboardSize = result.RectangleFValue.Size;
            if (Element != null)
            {
                var chatInputbarView = (ChatInputBarView)Element;

                _originalMargin = chatInputbarView.Margin;

                Element.Margin = new Thickness(_originalMargin.Left, _originalMargin.Top, _originalMargin.Right, _originalMargin.Bottom + keyboardSize.Height - chatInputbarView.PagePadding.Bottom); //push the entry up to keyboard height when keyboard is activated
                chatInputbarView.OnKeyboardActivated();
            }
        }

        private void OnKeyboardHide(object sender, UIKeyboardEventArgs e)
        {
            if (Element != null)
            {
                Element.Margin = _originalMargin; //set the margins to zero when keyboard is dismissed
            }
        }

        private void UnregisterForKeyboardNotifications()
        {
            if (_keyboardShowObserver != null)
            {
                _keyboardShowObserver.Dispose();
                _keyboardShowObserver = null;
            }

            if (_keyboardHideObserver != null)
            {
                _keyboardHideObserver.Dispose();
                _keyboardHideObserver = null;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
        }
    }
}
