namespace CoolapkLite.Models
{
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
