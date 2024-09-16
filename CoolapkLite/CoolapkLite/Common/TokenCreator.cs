using CoolapkLite.Helpers;
using CoolapkLite.Models.Network;
using System;

namespace CoolapkLite.Common
{
    /// <summary>
    /// Create a token for Coolapk.
    /// </summary>
    public class TokenCreator
    {
        /// <summary>
        /// Get or set the default device code.
        /// </summary>
        public static string DeviceCode { get; protected set; }

        /// <summary>
        /// The token version.
        /// </summary>
        private readonly TokenVersion TokenVersion;

        static TokenCreator() => DeviceCode = SettingsHelper.Get<DeviceInfo>(SettingsHelper.DeviceInfo).CreateDeviceCode();

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCreator"/> class.
        /// </summary>
        /// <param name="version">The token version.</param>
        public TokenCreator(TokenVersion version = TokenVersion.TokenV2) => TokenVersion = version;

        /// <summary>
        /// GetToken Generate a token with random device info.
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
        /// Generate a token v1 with your device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns>The generated token.</returns>
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

        /// <summary>
        /// Generate a token v2 with your device code.
        /// </summary>
        /// <param name="deviceCode">The device code.</param>
        /// <returns>The generated token.</returns>
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

        /// <summary>
        /// Update the device info.
        /// </summary>
        /// <param name="deviceInfo">The device info to update.</param>
        public static void UpdateDeviceInfo(DeviceInfo deviceInfo) => DeviceCode = deviceInfo.CreateDeviceCode();

        /// <inheritdoc/>
        public override string ToString() => GetToken();
    }

    public enum TokenVersion
    {
        TokenV1 = 1,
        TokenV2
    }
}
