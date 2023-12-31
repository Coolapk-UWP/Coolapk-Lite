using Newtonsoft.Json.Linq;
using System;

namespace CoolapkLite.Models.Exceptions
{
    public sealed class CoolapkMessageException : Exception
    {
        public const string RequestCaptcha = "err_request_captcha";
        public const string RequestCaptchaV2 = "err_request_captcha_v2";

        public CoolapkMessageException(string message) : base(message) { }

        public CoolapkMessageException(string message, Exception innerException) : base(message, innerException) { }

        public CoolapkMessageException(JObject token)
            : base(token?.TryGetValue("message", out JToken message) == true ? message.ToString() : string.Empty)
        {
            if (token != null && token.TryGetValue("messageStatus", out JToken messageStatus))
            {
                MessageStatus = messageStatus.ToString();
            }
        }

        public CoolapkMessageException(JObject token, Exception innerException)
            : base(token?.TryGetValue("message", out JToken message) == true ? message.ToString() : string.Empty, innerException)
        {
            if (token != null && token.TryGetValue("messageStatus", out JToken messageStatus))
            {
                MessageStatus = messageStatus.ToString();
            }
        }

        public string MessageStatus { get; }
        public bool IsRequestCaptcha => MessageStatus == RequestCaptcha;
    }
}
