namespace Social;

public class Repository
{
    public static Repository<Social.CommentCount> CommentCount
    {
        get
        {
            return new Repository<Social.CommentCount>(new SocialContext());
        }
    }

    public static Repository<Social.Comment> Comment
    {
        get
        {
            return new Repository<Social.Comment>(new SocialContext());
        }
    }

    public static Repository<Social.DislikeCount> DislikeCount
    {
        get
        {
            return new Repository<Social.DislikeCount>(new SocialContext());
        }
    }

    public static Repository<Social.Dislike> Dislike
    {
        get
        {
            return new Repository<Social.Dislike>(new SocialContext());
        }
    }

    public static Repository<Social.LikeCount> LikeCount
    {
        get
        {
            return new Repository<Social.LikeCount>(new SocialContext());
        }
    }

    public static Repository<Social.Like> Like
    {
        get
        {
            return new Repository<Social.Like>(new SocialContext());
        }
    }

    public static Repository<Social.ViewCount> ViewCount
    {
        get
        {
            return new Repository<Social.ViewCount>(new SocialContext());
        }
    }

    public static Repository<Social.View> View
    {
        get
        {
            return new Repository<Social.View>(new SocialContext());
        }
    }
}
