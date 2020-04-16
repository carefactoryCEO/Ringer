using System;
using System.Text.RegularExpressions;
using RingerStaff.Models;
using RingerStaff.Types;

namespace RingerStaff.Helpers
{
    public class Utility
    {
        public static MessageTypes GetMediaAndDirectionType(string body, int senderId, int userId, ref MessageModel lastMessage)
        {
            // set default
            MessageTypes messageTypes = MessageTypes.Text | MessageTypes.Leading | MessageTypes.Trailing;

            // set media type
            if (body != null)
            {
                string videoPattern = @"^https://ringerstoragekr.blob.core.windows.net/ringer/[\w-]+(?:\.mov|\.mp4)$";
                string imagePattern = @"^https://ringerstoragekr.blob.core.windows.net/ringer/[\w-]+\.jpg$";

                if (Regex.IsMatch(body, videoPattern))
                {
                    messageTypes |= MessageTypes.Video;
                    messageTypes ^= MessageTypes.Text;
                }

                if (Regex.IsMatch(body, imagePattern))
                {
                    messageTypes |= MessageTypes.Image;
                    messageTypes ^= MessageTypes.Text;
                }
            }

            // set direction
            messageTypes |= (senderId == userId) ? MessageTypes.Outgoing : MessageTypes.Incomming;

            return messageTypes;
        }

        public static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool InSameMinute(DateTime current, DateTime last)
        {
            return current - last < TimeSpan.FromMinutes(1) && current.Minute == last.Minute;
        }

        internal static void SetMessageTypes(ref MessageModel message, ref MessageModel lastMessage, int userId)
        {
            // set default
            MessageTypes messageTypes = MessageTypes.Text | MessageTypes.Leading | MessageTypes.Trailing;

            // set media type
            if (message.Body != null)
            {
                string videoPattern = @"^https://ringerstoragekr.blob.core.windows.net/ringer/[\w-]+(?:\.mov|\.mp4)$";
                string imagePattern = @"^https://ringerstoragekr.blob.core.windows.net/ringer/[\w-]+\.jpg$";

                if (Regex.IsMatch(message.Body, videoPattern))
                {
                    messageTypes |= MessageTypes.Video;
                    messageTypes ^= MessageTypes.Text;
                }

                if (Regex.IsMatch(message.Body, imagePattern))
                {
                    messageTypes |= MessageTypes.Image;
                    messageTypes ^= MessageTypes.Text;
                }
            }

            // set direction
            messageTypes |= (message.SenderId == userId) ? MessageTypes.Outgoing : MessageTypes.Incomming;

            message.MessageTypes = messageTypes;

            if (lastMessage != null)
            {
                if (lastMessage.Sender == message.Sender && InSameMinute(message.CreatedAt, lastMessage.CreatedAt))
                {
                    lastMessage.MessageTypes ^= MessageTypes.Trailing;
                    message.MessageTypes ^= MessageTypes.Leading;
                }
            }
        }
    }
}
