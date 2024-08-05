using CoolapkLite.Helpers;
using CoolapkLite.Models.Network;
using System;

namespace CoolapkLite.Common
{
    public class TokenCreator
    {
        public static string DeviceCode { get; protected set; }

        private readonly TokenVersion TokenVersion;

        static TokenCreator() => DeviceCode = SettingsHelper.Get<DeviceInfo>(SettingsHelper.DeviceInfo).CreateDeviceCode();

        public TokenCreator(TokenVersion version = TokenVersion.TokenV2) => TokenVersion = version;

        /// <summary>
        /// GetToken Generate a token with random device info
        /// </summary>
        public string GetToken()
        {
            switch (TokenVersion)
            {
                case TokenVersion.TokenV1:
                    return GetCoolapkAppToken(DeviceCode);
                default:
                case TokenVersion.TokenV2:
                    return GetTokenWithDeviceCode(DeviceCode);
            }
        }

        /// <summary>
        /// GetTokenWithDeviceCode Generate a token with your device code
        /// </summary>
        private static string GetTokenWithDeviceCode(string deviceCode)
        {
            string timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            string base64TimeStamp = timeStamp.GetBase64();
            string md5TimeStamp = timeStamp.GetMD5();
            string md5DeviceCode = deviceCode.GetMD5();

            string token = $"token://com.coolapk.market/dcf01e569c1e3db93a3d0fcf191a622c?{md5TimeStamp}${md5DeviceCode}&com.coolapk.market";
            string base64Token = token.GetBase64();
            string md5Base64Token = base64Token.GetMD5();
            string md5Token = token.GetMD5();

            string bcryptSalt = $"{$"$2y$10${base64TimeStamp}/{md5Token}".Substring(0, 31)}u";
            string bcryptResult = BCrypt.Net.BCrypt.HashPassword(md5Base64Token, bcryptSalt);

            string appToken = $"v2{bcryptResult.GetBase64()}";
            return appToken;
        }

        private static string GetCoolapkAppToken(string deviceCode)
        {
            long timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string hex_timeStamp = $"0x{timeStamp:x}";

            // 时间戳加密
            string md5_timeStamp = timeStamp.ToString().GetMD5();
            string md5_deviceCode = deviceCode.GetMD5();

            string token = $"token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?{md5_timeStamp}${md5_deviceCode}&com.coolapk.market";
            string md5_token = token.GetBase64(true).GetMD5();

            string appToken = $"{md5_token}{md5_deviceCode}{hex_timeStamp}";
            return appToken;
        }

        public static void UpdateDeviceInfo(DeviceInfo deviceInfo) => DeviceCode = deviceInfo.CreateDeviceCode();

        public override string ToString() => GetToken();
    }

    public enum TokenVersion
    {
        TokenV1 = 1,
        TokenV2
    }
}
