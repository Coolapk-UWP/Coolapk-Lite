using System;

namespace CoolapkLite.Models.Network
{
    public struct Account : IEquatable<Account>
    {
        public string UID { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(UID) || string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Token);

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

        public override bool Equals(object obj) => obj is Account other && Equals(other);

        public override int GetHashCode() => (UID, UserName, Token).GetHashCode();

        public bool Equals(Account other) => UID == other.UID && UserName == other.UserName && Token == other.Token;

        public static bool operator ==(Account left, Account right) => left.Equals(right);

        public static bool operator !=(Account left, Account right) => !(left == right);
    }
}
