using CoolapkLite.Models.Images;

namespace CoolapkLite.Models.Users
{
    public class LinkUserModel : ISourceUserModel, IHasTitle
    {
        public string Url { get; set; }
        public string Title => UserName;
        public string UserName { get; set; }
        public ImageModel UserAvatar { get; set; }
    }
}
