using CoolapkLite.Models.Images;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace CoolapkLite.Models
{
    public interface IHasUrl
    {
        string Url { get; }
    }

    public interface IHasTitle : IHasUrl
    {
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

    public interface ISourceUserModel : IHasUrl
    {
        string UserName { get; }
        ImageModel UserAvatar { get; }
    }

    public interface ISourceFeedModel : ICanCopy, IHasUrl
    {
        bool ShowUser { get; set; }
        string Message { get; }
        string Dateline { get; }
        string MessageTitle { get; }
        DateTime DateTime { get; }
        ISourceUserModel UserInfo { get; }
        ImmutableArray<ImageModel> PicArr { get; }
    }
}
