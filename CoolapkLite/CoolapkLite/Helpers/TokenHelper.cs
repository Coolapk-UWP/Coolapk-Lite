using System;

namespace CoolapkLite.Helpers
{
	public class TokenHelper
	{
		private string manufactor = "Google";
        private string brand = "Google";
        private string[] model = new string[]
		{
			"Pixel 3",
			"Pixel 3 XL",
			"Pixel 3a",
			"Pixel 4",
			"Pixel 4 XL",
			"Pixel 4a",
			"Pixel 5",
			"Pixel 5a",
		};
        private string[] buildNumber = new string[]
		{
			"SQ1A.220105.002",
			"SQ1D.220105.007",
		};

        /// <summary>
        /// GetToken Generate a token with random device info
        /// </summary>
        public (string randDeviceCode, string token) GetToken()
		{
			var rand = new Random();
			var randDeviceCode = CreateDeviceCode(randHexString(16), randMacAdress(), manufactor, brand, model[rand.Next(model.Length)], buildNumber[rand.Next(buildNumber.Length)]);

			return (randDeviceCode, GetTokenWithDeviceCode(randDeviceCode));
		}

        /// <summary>
        /// GetTokenWithDeviceCode Generate a token with your device code
        /// </summary>
        private string GetTokenWithDeviceCode(string deviceCode)
		{
			var timeStamp = DateTime.Now.ConvertDateTimeToUnixTimeStamp().ToString();

			var base64TimeStamp = timeStamp.GetBase64(true);
			var md5TimeStamp = timeStamp.GetMD5().ToLowerInvariant();
			var md5DeviceCode = deviceCode.GetMD5().ToLowerInvariant();

			var token = $"token://com.coolapk.market/dcf01e569c1e3db93a3d0fcf191a622c?{md5TimeStamp}${md5DeviceCode}&com.coolapk.market";
			var base64Token = token.GetBase64(true);
			var md5Base64Token = base64Token.GetMD5().ToLowerInvariant();
			var md5Token = token.GetMD5().ToLowerInvariant();

			var bcryptSalt = $"{$"$2y$10${base64TimeStamp}/{md5Token}".Substring(0,31)}u";
			var bcryptresult = BCrypt.Net.BCrypt.HashPassword(md5Base64Token, bcryptSalt);

			var appToken = $"v2{bcryptresult.GetBase64(true)}";

			return appToken;
		}

        /// <summary>
        /// CreateDeviceCode Generace your custom device code
        /// </summary>
        private string CreateDeviceCode(string aid, string mac, string manufactor, string brand, string model, string buildNumber)
		{
			return $"{aid}; ; ; {mac}; {manufactor}; {brand}; {model}; {buildNumber}".GetBase64(true).Reverse();
		}

        private string randMacAdress()
		{
			var rand = new Random();
			string macAdress = string.Empty;
			for (int i = 0; i < 6; i++)
			{
                macAdress += rand.Next(256).ToString("x2");
				if (i != 5)
				{
					macAdress += ":";
				}
			}
			return macAdress;
		}

		private string randHexString(int n)
		{
			var rand = new Random();
			var bytes = new byte[n];
			rand.NextBytes(bytes);
			return BitConverter.ToString(bytes).ToUpperInvariant().Replace("-", "");
		}
	}
}
