using CoolapkLite.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Text;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace CoolapkLite.Common
{
    public class TokenCreator
    {
        //private static readonly char[] constant = new[]
        //{
        //    '0','1','2','3','4','5','6','7','8','9',
        //    'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
        //    'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
        //};

        private static readonly string guid = Guid.NewGuid().ToString();
        //private static readonly string aid = $"DU{RandHexString(14)}_{RandHexString(19)}";
        private static readonly string aid = RandHexString(16);
        private static readonly string mac = RandMacAddress();
        private static readonly string SystemManufacturer;
        private static readonly string SystemProductName;

        public static string DeviceCode;

        private readonly TokenVersions TokenVersion;

        static TokenCreator()
        {
            EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
            SystemManufacturer = deviceInfo.SystemManufacturer;
            SystemProductName = deviceInfo.SystemProductName;
            DeviceCode = CreateDeviceCode(aid, mac, SystemManufacturer, SystemManufacturer, SystemProductName, SystemInformation.Instance.OperatingSystemVersion.ToString());
        }

        public TokenCreator(TokenVersions version = TokenVersions.TokenV2) => TokenVersion = version;

        /// <summary>
        /// GetToken Generate a token with random device info
        /// </summary>
        public string GetToken()
        {
            switch (TokenVersion)
            {
                case TokenVersions.TokenV1:
                    return GetCoolapkAppToken();
                default:
                case TokenVersions.TokenV2:
                    return GetTokenWithDeviceCode(DeviceCode);
            }
        }

        /// <summary>
        /// GetTokenWithDeviceCode Generate a token with your device code
        /// </summary>
        private string GetTokenWithDeviceCode(string deviceCode)
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

        private static string GetCoolapkAppToken()
        {
            long timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string hex_timeStamp = $"0x{Convert.ToString((int)timeStamp, 16)}";
            // 时间戳加密
            string md5_timeStamp = $"{timeStamp}".GetMD5();
            string token = $"token://com.coolapk.market/c67ef5943784d09750dcfbb31020f0ab?{md5_timeStamp}${guid}&com.coolapk.market";
            string md5_token = token.GetBase64(true).GetMD5();
            string appToken = $"{md5_token}{guid}{hex_timeStamp}";
            return appToken;
        }

        /// <summary>
        /// CreateDeviceCode Generate your custom device code
        /// </summary>
        private static string CreateDeviceCode(string aid, string mac, string manufactory, string brand, string model, string buildNumber) =>
            $"{aid}; ; ; {mac}; {manufactory}; {brand}; {model}; {buildNumber}; null".GetBase64().Reverse();

        private static string RandMacAddress()
        {
            Random rand = new Random();
            string macAddress = string.Empty;
            for (int i = 0; i < 6; i++)
            {
                macAddress += rand.Next(256).ToString("x2");
                if (i != 5)
                {
                    macAddress += ":";
                }
            }
            return macAddress;
        }

        private static string RandHexString(int length)
        {
            //StringBuilder newRandom = new StringBuilder(62);
            //Random random = new Random((int)DateTime.Now.Ticks);
            //for (int i = 0; i < length; i++)
            //{
            //    _ = newRandom.Append(constant[random.Next(62)]);
            //}
            //return newRandom.ToString();
            Random rand = new Random((int)DateTime.Now.Ticks);
            byte[] bytes = new byte[length];
            rand.NextBytes(bytes);
            return BitConverter.ToString(bytes).ToUpperInvariant().Replace("-", "");
        }

        public override string ToString() => GetToken();
    }

    public enum TokenVersions
    {
        TokenV1,
        TokenV2
    }

    public enum APIVersions
    {
        Custom = 4,
        小程序,
        V6,
        V7,
        V8,
        V9,
        V10,
        V11,
        V12,
        V13,
    }
}
