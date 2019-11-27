namespace Ringer.Core.EventArgs
{
    public class ConnectionEventArgs : IChatEventArgs
    {
        public string Message { get; private set; }

        public ConnectionEventArgs(string message)
        {
            Message = message;
        }
    }
}