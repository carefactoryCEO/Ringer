namespace Ringer.Core.EventArgs
{
    public class SignalREventArgs : IChatEventArgs
    {
        public SignalREventArgs(string message, string user)
        {
            Message = message;
            User = user;
        }

        public string Message { get; }
        public string User { get; }
    }
}