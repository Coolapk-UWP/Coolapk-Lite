using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Windows.Foundation.Metadata;

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
            if (token != null)
            {
                if (token.TryGetValue("messageStatus", out JToken messageStatus))
                {
                    MessageStatus = messageStatus.ToString();
                }
                if (token.TryGetValue("messageExtra", out JToken messageExtra)
                    || token.TryGetValue("extra", out messageExtra))
                {
                    MessageExtra = messageExtra.ToString();
                }
            }
        }

        public CoolapkMessageException(JObject token, Exception innerException)
            : base(token?.TryGetValue("message", out JToken message) == true ? message.ToString() : string.Empty, innerException)
        {
            if (token != null)
            {
                if (token.TryGetValue("messageStatus", out JToken messageStatus))
                {
                    MessageStatus = messageStatus.ToString();
                }
                if (token.TryGetValue("messageExtra", out JToken messageExtra)
                    || token.TryGetValue("extra", out messageExtra))
                {
                    MessageExtra = messageExtra.ToString();
                }
            }
        }

        public CaptchaChallenge GetCaptchaChallenge()
        {
            string id = CaptchaChallenge.DefaultCaptchaID;
            string field = CaptchaChallenge.DefaultCaptchaField;
            if (IsRequestCaptchaV2 && !string.IsNullOrEmpty(MessageExtra))
            {
                JObject token = JObject.Parse(MessageExtra);
                if (token.TryGetValue("captchaId", out JToken captchaId))
                {
                    string _id = captchaId.ToString();
                    if (_id.Length >= 16 && _id.Length <= 64 &&
                        _id.All(x => x >= 'a' && x <= 'z' ||
                                     x >= '0' && x <= '9' ||
                                     x >= 'A' && x <= 'Z' ))
                    {
                        id = _id;
                    }
                }
                if (token.TryGetValue("captchaField", out JToken captchaField))
                {
                    string _field = captchaField.ToString();
                    if (_field.Length > 1 && _field.Length <= 64 &&
                        _field[0] == '_' &&
                        _field.Skip(1).All(x =>
                            x >= 'a' && x <= 'z' ||
                            x >= '0' && x <= '9' ||
                            x >= 'A' && x <= 'Z' || x == '_'))
                    {
                        field = _field;
                    }
                }
            }
            return new CaptchaChallenge(id, field);
        }

        public string MessageStatus { get; }
        public string MessageExtra { get; }
        public bool IsRequestCaptcha => MessageStatus == RequestCaptcha;
        public bool IsRequestCaptchaV2 => MessageStatus == RequestCaptchaV2;
    }

    public sealed class CaptchaChallenge
    {
        public const string DefaultCaptchaID = "414e5c9b866a03db03f860a1a9101672";
        public const string DefaultCaptchaField = "_v2_post_token";
        public string CaptchaID { get; }
        public string CaptchaField { get; }
        public CaptchaChallenge(string captchaId, string captchaField)
        {
            CaptchaID = captchaId;
            CaptchaField = captchaField;
        }
    }
}
