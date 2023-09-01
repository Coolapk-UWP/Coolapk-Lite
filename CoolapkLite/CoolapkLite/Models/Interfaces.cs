using CoolapkLite.Helpers;
using CoolapkLite.Models.Images;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace CoolapkLite.Models
{
    public interface IPic
    {
        string Uri { get; }
        ImageType Type { get; }
        BitmapImage Pic { get; }
    }

    public interface IHasTitle
    {
        string Url { get; }
        string Title { get; }
    }

    public interface IHasDescription : IHasTitle
    {
        ImageModel Pic { get; }
        string Description { get; }
    }

    public interface IHasSubtitle : IHasDescription
    {
        string SubTitle { get; }
    }

    public interface ICanCopy
    {
        bool IsCopyEnabled { get; set; }
    }

    public interface ICanLike
    {
        int ID { get; }
        bool Liked { get; set; }
        Task ChangeLikeAsync();
    }

    public interface ICanStar
    {
        int ID { get; }
        bool Stared { get; set; }
        int StarNum { get; set; }
    }

    public interface ICanReply
    {
        int ID { get; }
        int ReplyNum { get; set; }
    }

    public interface ICanFollow
    {
        int ID { get; }
        bool Followed { get; set; }
        Task ChangeFollowAsync();
    }

    public interface IUserModel : ISourceUserModel
    {
        int FansNum { get; }
        int FollowNum { get; }

        string Bio { get; }
        string LoginText { get; }

        ImageModel Cover { get; }
    }

    public interface ISourceUserModel
    {
        string Url { get; }
        string UserName { get; }
        ImageModel UserAvatar { get; }
    }

    public interface ISourceFeedModel : ICanCopy
    {
        bool ShowUser { get; set; }
        string Url { get; }
        string Message { get; }
        string Dateline { get; }
        string MessageTitle { get; }
        DateTime DateTime { get; }
        ISourceUserModel UserInfo { get; }
        ImmutableArray<ImageModel> PicArr { get; }
    }
}
