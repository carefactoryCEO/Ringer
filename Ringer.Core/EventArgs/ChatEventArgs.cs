namespace Ringer.Core.EventArgs
{
    public class ChatEventArgs : IChatEventArgs
    {

        public ChatEventArgs(string message, string user)
        {
            Message = message;
            User = user;
        }

        public string Message { get; }
        public string User { get; }
    }
}