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
        int ID { get; }
        int ReplyNum { get; set; }
    }

    internal interface ICanChangeLikeModel
    {
        int ID { get; }
        bool Liked { get; set; }
        int LikeNum { get; set; }
    }

    internal interface ICanChangeStarModel
    {
        int ID { get; }
        bool Stared { get; set; }
        int StarNum { get; set; }
    }
}
