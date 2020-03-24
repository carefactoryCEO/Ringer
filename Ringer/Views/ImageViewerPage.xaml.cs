using System;
using System.Diagnostics;
using Ringer.Extensions;
using Xamarin.Forms;

namespace Ringer.Views
{
    public partial class ImageViewerPage : ContentPage
    {
        double currentScale = 1;
        double startScale = 1;
        double xOffset = 0;
        double yOffset = 0;

        public ImageViewerPage()
        {
            InitializeComponent();
        }

        public ImageViewerPage(string url) : this()
        {
            imageViewer.Source = url;
        }

        void PinchGestureRecognizer_PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                startScale = imageViewer.Scale;
                imageViewer.AnchorX = 0;
                imageViewer.AnchorY = 0;
            }
            if (e.Status == GestureStatus.Running)
            {
                currentScale += (e.Scale - 1) * startScale;
                currentScale = Math.Max(1, currentScale);

                double renderedX = imageViewer.X + xOffset;
                double deltaX = renderedX / Width;
                double deltaWidth = Width / (imageViewer.Width * startScale);
                double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                double renderedY = imageViewer.Y + yOffset;
                double deltaY = renderedY / Height;
                double deltaHeight = Height / (imageViewer.Height * startScale);
                double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                double targetX = xOffset - (originX * imageViewer.Width) * (currentScale - startScale);
                double targetY = yOffset - (originY * imageViewer.Height) * (currentScale - startScale);
                imageViewer.TranslationX = targetX.Clamp(-imageViewer.Width * (currentScale - 1), 0);
                imageViewer.TranslationY = targetY.Clamp(-imageViewer.Height * (currentScale - 1), 0);

                imageViewer.Scale = currentScale;
            }

            if (e.Status == GestureStatus.Completed)
            {
                xOffset = imageViewer.TranslationX;
                yOffset = imageViewer.TranslationY;
            }
        }
        void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (imageViewer.Scale <= 1)
                return;

            if (e.StatusType == GestureStatus.Running)
            {
                imageViewer.TranslationX = xOffset + e.TotalX;
                imageViewer.TranslationY = yOffset + e.TotalY;
                //Debug.WriteLine($"type: {e.StatusType}, total: ({e.TotalX}, {e.TotalY}), cor: ({imageViewer.TranslationX},{imageViewer.TranslationY})");
            }

            if (e.StatusType == GestureStatus.Completed)
            {
                xOffset = imageViewer.TranslationX;
                yOffset = imageViewer.TranslationY;
            }

        }
        void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            Debug.WriteLine(e.Direction);

        }
        void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            imageViewer.ScaleTo(1, 250);
            imageViewer.TranslateTo(0, 0, 250);
            xOffset = 0;
            yOffset = 0;
            currentScale = 1;
        }

        /*
		void PinchGestureRecognizer_PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            //if (e.Status == GestureStatus.Started)
            //{
            //    _d1 = new Point((e.ScaleOrigin.X - 0.5) * imageViewer.Width, (e.ScaleOrigin.Y - 0.5) * imageViewer.Height);
            //}

            //if (e.Status == GestureStatus.Running)
            //{
            //    var scale = imageViewer.Scale * e.Scale;
            //    imageViewer.Scale = scale;

            //    _d2 = new Point(_d1.X * imageViewer.Scale, _d1.Y * imageViewer.Scale);

            //    var delta = new Point(_d2.X - _d1.X, _d2.Y - _d1.Y);

            //    imageViewer.TranslationX = -delta.X;
            //    imageViewer.TranslationY = -delta.Y;

            //    Debug.WriteLine($"{imageViewer.Scale}, {_d1}, {_d2}, {delta}");
            //}

            //if (e.Status == GestureStatus.Completed)
            //{
            //    imageViewer.TranslateTo(0, 0, 250, Easing.Linear);
            //    imageViewer.ScaleTo(1, 250, Easing.Linear);
            //}
        }
        */
    }
}
