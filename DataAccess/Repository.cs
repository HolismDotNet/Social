namespace Holism.Social.DataAccess
{
    public class RepositoryFactory
    {
        public static Repositories.CommentRepository Comment
        {
            get
            {
                return new Repositories.CommentRepository();
            }
        }

        public static Repositories.CommentRepository CommentFrom(string databaseName = null)
        {
            return new Repositories.CommentRepository(databaseName);
        }

        public static Repositories.CommentCountRepository CommentCount
        {
            get
            {
                return new Repositories.CommentCountRepository();
            }
        }

        public static Repositories.CommentCountRepository CommentCountFrom(string databaseName = null)
        {
            return new Repositories.CommentCountRepository(databaseName);
        }

        public static Repositories.DislikeRepository Dislike
        {
            get
            {
                return new Repositories.DislikeRepository();
            }
        }

        public static Repositories.DislikeRepository DislikeFrom(string databaseName = null)
        {
            return new Repositories.DislikeRepository(databaseName);
        }

        public static Repositories.DislikeCountRepository DislikeCount
        {
            get
            {
                return new Repositories.DislikeCountRepository();
            }
        }

        public static Repositories.DislikeCountRepository DislikeCountFrom(string databaseName = null)
        {
            return new Repositories.DislikeCountRepository(databaseName);
        }

        public static Repositories.LikeRepository Like
        {
            get
            {
                return new Repositories.LikeRepository();
            }
        }

        public static Repositories.LikeRepository LikeFrom(string databaseName = null)
        {
            return new Repositories.LikeRepository(databaseName);
        }

        public static Repositories.LikeCountRepository LikeCount
        {
            get
            {
                return new Repositories.LikeCountRepository();
            }
        }

        public static Repositories.LikeCountRepository LikeCountFrom(string databaseName = null)
        {
            return new Repositories.LikeCountRepository(databaseName);
        }

        public static Repositories.ViewRepository View
        {
            get
            {
                return new Repositories.ViewRepository();
            }
        }

        public static Repositories.ViewRepository ViewFrom(string databaseName = null)
        {
            return new Repositories.ViewRepository(databaseName);
        }

        public static Repositories.ViewCountRepository ViewCount
        {
            get
            {
                return new Repositories.ViewCountRepository();
            }
        }

        public static Repositories.ViewCountRepository ViewCountFrom(string databaseName = null)
        {
            return new Repositories.ViewCountRepository(databaseName);
        }
    }
}
