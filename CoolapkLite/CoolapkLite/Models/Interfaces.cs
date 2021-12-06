using CoolapkLite.Models.Images;

namespace CoolapkLite.Models
{
    internal interface IList
    {
        string Url { get; }
        string Title { get; }
        string Description { get; }
        BackgroundImageModel Pic { get; }
    }

    internal interface IHasUriAndTitle
    {
        string Url { get; }
        string Title { get; }
    }

    internal interface IListWithSubtitle
    {
        string Url { get; }
        string Title { get; }
        string SubTitle { get; }
        string Description { get; }
        BackgroundImageModel Pic { get; }
    }

    internal interface ICanCopy
    {
        bool IsCopyEnabled { get; set; }
    }

    internal interface ICanChangeReplyNum
    {
        string ReplyNum { get; set; }
    }

    internal interface ICanChangeLikeModel
    {
        string ID { get; }
        bool Liked { get; set; }
        string LikeNum { get; set; }
    }
}
