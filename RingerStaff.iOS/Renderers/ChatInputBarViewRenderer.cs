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

                Element.Margin = new Thickness(0, 0, 0, keyboardSize.Height - chatInputbarView.Padding.Bottom); //push the entry up to keyboard height when keyboard is activated
                chatInputbarView.NotifyListScroll();
            }
        }

        private void OnKeyboardHide(object sender, UIKeyboardEventArgs e)
        {
            if (Element != null)
            {
                Element.Margin = new Thickness(); //set the margins to zero when keyboard is dismissed
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
    }
}
