namespace Social;

public class Helper
{
    public void RemoveEntity(string entityType, Guid entityGuid)
    {
        new LikeBusiness().RemoveLikes(entityType, entityGuid);
        new LikeCountBusiness().RemoveLikeCount(entityType, entityGuid);
        new DislikeBusiness().RemoveDislikes(entityType, entityGuid);
        new DislikeCountBusiness().RemoveDislikeCount(entityType, entityGuid);
        new CommentBusiness().RemoveComments(entityType, entityGuid);
        new CommentCountBusiness().RemoveCommentCount(entityType, entityGuid);
        new ReadBusiness().RemoveViews(entityType, entityGuid);
        new ViewCountBusiness().RemoveViewCount(entityType, entityGuid);
        //new ExcludedEntityBusiness().Include(entityType, entityGuid);
    }

    public void RemoveOrphanEntities(string entityType, List<Guid> entityGuids)
    {
        new LikeBusiness().RemoveOrphanEntities(entityType, entityGuids);
        new LikeCountBusiness().RemoveOrphanEntities(entityType, entityGuids);
        new DislikeBusiness().RemoveOrphanEntities(entityType, entityGuids);
        new DislikeCountBusiness().RemoveOrphanEntities(entityType, entityGuids);
        new CommentBusiness().RemoveOrphanEntities(entityType, entityGuids);
        new CommentCountBusiness().RemoveOrphanEntities(entityType, entityGuids);
        new ReadBusiness().RemoveOrphanEntities(entityType, entityGuids);
        new ViewCountBusiness().RemoveOrphanEntities(entityType, entityGuids);
        //new ExcludedEntityBusiness().RemoveOrphanEntities(entityType, entityGuids);
    }

    public void Inflate(string entityType, object entity,Guid userGuid, params SocialItem[] socialItems)
    {
        foreach (var socialItem in socialItems)
        {
            switch (socialItem)
            {
                case SocialItem.Like:
                    new LikeBusiness().InflateWithLikesInfo(entityType, entity, userGuid);
                    break;
                case SocialItem.Dislike:
                    new DislikeBusiness().InflateWithDislikesInfo(entityType, entity, userGuid);
                    break;
                case SocialItem.View:
                    new ReadBusiness().InflateWithViewsInfo(entityType, entity, userGuid);
                    break;
                case SocialItem.Comment:
                    break;
                default:
                    break;
            }
        }
    }
}
