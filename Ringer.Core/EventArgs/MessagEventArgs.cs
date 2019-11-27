namespace Ringer.Core.EventArgs
{
    public class MessagEventArgs : IChatEventArgs
    {
        public MessagEventArgs(string message, string user)
        {
            Message = message;
            User = user;
        }

        public string Message { get; }
        public string User { get; }
    }
}