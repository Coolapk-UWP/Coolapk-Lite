using CoolapkLite.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace CoolapkLite.Models.Upload
{
    public class UploadFileFragment
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        public static async Task<UploadFileFragment> FromPictureFile(StorageFile file)
        {
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                var img = await BitmapDecoder.CreateAsync(stream);

                return new UploadFileFragment
                {
                    Name = file.Name,
                    Resolution = $"{img.PixelWidth}x{img.PixelHeight}",
                    Md5 = await GetMD5Hash(file),
                };
            }
        }

        public static async Task<string> GetMD5Hash(StorageFile file)
        {
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                var Provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
                var computedHash = Provider.HashData(stream.GetBuffer());
                string result = CryptographicBuffer.EncodeToHexString(computedHash);
                return result;
            }
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            return obj is UploadFileFragment && Md5.Equals((obj as UploadFileFragment).Md5);
        }
    }
}
