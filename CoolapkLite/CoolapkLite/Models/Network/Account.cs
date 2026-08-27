namespace CoolapkLite.Models.Network
{
    public class Account
    {
        public string UID { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(UID) || string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Token);

        public Account() { }

        public Account(string uid, string username, string token) : this()
        {
            UID = uid;
            UserName = username;
            Token = token;
        }

        public void Deconstruct(out string uid, out string username, out string token)
        {
            uid = UID;
            username = UserName;
            token = Token;
        }
    }
}
