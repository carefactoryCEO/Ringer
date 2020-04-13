namespace Ringer.Core.Data
{
    public class LoginResponse
    {
        public string token { get; set; }
        public string roomId { get; set; }
        public int userId { get; set; }
        public bool success { get; set; }
        public string userName { get; set; }
    }
}
