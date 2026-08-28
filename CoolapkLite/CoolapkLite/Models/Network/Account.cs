using System;
using System.Collections.Generic;

namespace CoolapkLite.Models.Network
{
    public class Account : IEquatable<Account>
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

        public override bool Equals(object obj) => Equals(obj as Account);

        public override int GetHashCode() => (UID, UserName, Token).GetHashCode();

        public bool Equals(Account other) => other is Account && UID == other.UID && UserName == other.UserName && Token == other.Token;

        public static bool operator ==(Account left, Account right) => EqualityComparer<Account>.Default.Equals(left, right);

        public static bool operator !=(Account left, Account right) => !(left == right);
    }
}
