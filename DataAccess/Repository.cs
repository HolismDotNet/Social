using Holism.Social.Models;
using Holism.DataAccess;

namespace Holism.Social.DataAccess
{
    public class Repository
    {
        public static Repository<Comment> Comment
        {
            get
            {
                return new Holism.DataAccess.Repository<Comment>(new SocialContext());
            }
        }

        public static Repository<CommentCount> CommentCount
        {
            get
            {
                return new Holism.DataAccess.Repository<CommentCount>(new SocialContext());
            }
        }

        public static Repository<Dislike> Dislike
        {
            get
            {
                return new Holism.DataAccess.Repository<Dislike>(new SocialContext());
            }
        }

        public static Repository<DislikeCount> DislikeCount
        {
            get
            {
                return new Holism.DataAccess.Repository<DislikeCount>(new SocialContext());
            }
        }

        public static Repository<Like> Like
        {
            get
            {
                return new Holism.DataAccess.Repository<Like>(new SocialContext());
            }
        }

        public static Repository<LikeCount> LikeCount
        {
            get
            {
                return new Holism.DataAccess.Repository<LikeCount>(new SocialContext());
            }
        }

        public static Repository<View> View
        {
            get
            {
                return new Holism.DataAccess.Repository<View>(new SocialContext());
            }
        }

        public static Repository<ViewCount> ViewCount
        {
            get
            {
                return new Holism.DataAccess.Repository<ViewCount>(new SocialContext());
            }
        }
    }
}