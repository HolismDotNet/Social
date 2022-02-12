namespace Social;

public class Repository
{
    public static Write<Social.CommentCount> CommentCount
    {
        get
        {
            return new Write<Social.CommentCount>(new SocialContext());
        }
    }

    public static Write<Social.Comment> Comment
    {
        get
        {
            return new Write<Social.Comment>(new SocialContext());
        }
    }

    public static Write<Social.DislikeCount> DislikeCount
    {
        get
        {
            return new Write<Social.DislikeCount>(new SocialContext());
        }
    }

    public static Write<Social.Dislike> Dislike
    {
        get
        {
            return new Write<Social.Dislike>(new SocialContext());
        }
    }

    public static Write<Social.LikeCount> LikeCount
    {
        get
        {
            return new Write<Social.LikeCount>(new SocialContext());
        }
    }

    public static Write<Social.Like> Like
    {
        get
        {
            return new Write<Social.Like>(new SocialContext());
        }
    }

    public static Write<Social.ViewCount> ViewCount
    {
        get
        {
            return new Write<Social.ViewCount>(new SocialContext());
        }
    }

    public static Write<Social.View> View
    {
        get
        {
            return new Write<Social.View>(new SocialContext());
        }
    }
}
