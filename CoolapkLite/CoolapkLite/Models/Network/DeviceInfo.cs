using CoolapkLite.Common;
using CoolapkLite.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Linq;
using System.Text;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace CoolapkLite.Models.Network
{
    public class DeviceInfo
    {
        private const string random = "Random";
        //private static readonly char[] constant = new char[62]
        //{
        //    '0','1','2','3','4','5','6','7','8','9',
        //    'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
        //    'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
        //};

        public static DeviceInfo Default
        {
            get
            {
                EasClientDeviceInformation deviceInfo = new EasClientDeviceInformation();
                return new DeviceInfo
                {
                    AndroidID = random,
                    MAC = random,
                    Manufactory = deviceInfo.SystemManufacturer,
                    Brand = deviceInfo.SystemManufacturer,
                    Model = deviceInfo.SystemProductName,
                    BuildNumber = SystemInformation.Instance.OperatingSystemVersion.ToString()
                };
            }
        }

        public string AndroidID { get; set; }
        public string MAC { get; set; }
        public string Manufactory { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string BuildNumber { get; set; }

        public DeviceInfo() { }

        public DeviceInfo(string aid, string mac, string manufactory, string brand, string model, string buildNumber) : this()
        {
            AndroidID = aid;
            MAC = mac;
            Manufactory = manufactory;
            Brand = brand;
            Model = model;
            BuildNumber = buildNumber;
        }

        public string CreateDeviceCode() => ToString().GetBase64().Reverse();

        public static DeviceInfo CreateByDeviceCode(string deviceCode)
        {
            deviceCode = deviceCode.Reverse();
            int index = deviceCode.Length % 4;
            byte[] bytes = Convert.FromBase64String(index == 0 ? deviceCode : $"{deviceCode}{new string(System.Linq.Enumerable.Repeat('=', 4 - index).ToArray())}");
            string result = Encoding.UTF8.GetString(bytes);
            string[] split = result.Split(';');
            if (split.Length < 8) { return null; }
            return new DeviceInfo(split[0].Trim(), split[3].Trim(), split[4].Trim(), split[5].Trim(), split[6].Trim(), split[7].Trim());
        }

        private static string RandMacAddress()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            byte[] bytes = new byte[6];
            rand.NextBytes(bytes);
            return string.Join(":", bytes.Select(x => x.ToString("x2")));
        }

        private static string RandHexString(int length)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            byte[] bytes = new byte[length];
            rand.NextBytes(bytes);
            return BitConverter.ToString(bytes).ToUpperInvariant().Replace("-", string.Empty);
        }

        //private static string RandString(int length)
        //{
        //    StringBuilder newRandom = new StringBuilder(length);
        //    Random random = new Random((int)DateTime.Now.Ticks);
        //    for (int i = 0; i < length; i++)
        //    {
        //        _ = newRandom.Append(constant[random.Next(62)]);
        //    }
        //    return newRandom.ToString();
        //}

        public override string ToString() => string.Join("; ", AndroidID == random ? RandHexString(16) : AndroidID, string.Empty, string.Empty, MAC == random ? RandMacAddress() : MAC, Manufactory, Brand, Model, BuildNumber, "null");
    }
}
