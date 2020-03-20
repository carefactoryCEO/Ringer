using Ringer.Models;
using Ringer.Types;
using Ringer.Views.Cells;
using Xamarin.Forms;

namespace Ringer.Helpers
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate entranceDataTemplage;
        private readonly DataTemplate imageDataTemplate;
        private readonly DataTemplate textDataTemplate;
        private readonly DataTemplate videoDataTemplate;

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

            message.MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Trailing;
            return textDataTemplate;
        }

    }
}
