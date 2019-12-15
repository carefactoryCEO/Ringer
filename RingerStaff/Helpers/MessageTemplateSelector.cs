using RingerStaff.Models;
using RingerStaff.Types;
using RingerStaff.Views.Cells;
using Xamarin.Forms;

namespace RingerStaff.Helpers
{
    class MessageTemplateSelector : DataTemplateSelector
    {
        private DataTemplate entranceDataTemplage;
        private DataTemplate imageDataTemplate;
        private DataTemplate textDataTemplate;
        private DataTemplate videoDataTemplate;

        public MessageTemplateSelector()
        {
            entranceDataTemplage = new DataTemplate(typeof(EntranceViewCell));
            imageDataTemplate = new DataTemplate(typeof(ImageViewCell));
            textDataTemplate = new DataTemplate(typeof(TextViewCell));
            videoDataTemplate = new DataTemplate(typeof(VideoViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var message = item as MessageModel;

            if (message == null)
                return null;

            if (message.MessageTypes.HasFlag(MessageTypes.Image))
                return imageDataTemplate;

            if (message.MessageTypes.HasFlag(MessageTypes.Video))
                return videoDataTemplate;

            if (message.MessageTypes.HasFlag(MessageTypes.Text))
                return textDataTemplate;

            if (message.MessageTypes.HasFlag(MessageTypes.EntranceNotice))
                return entranceDataTemplage;

            return null;
        }

    }
}