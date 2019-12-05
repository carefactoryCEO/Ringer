namespace Ringer.Core.EventArgs
{
    public class SignalREventArgs : IChatEventArgs
    {
        public SignalREventArgs(string message, string user)
        {
            Message = message;
            Sender = user;
        }

        public string Message { get; }
        public string Sender { get; }
    }
}