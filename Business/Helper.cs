using System;
using System.Collections.Generic;
using System.Text;

namespace Holism.Social.Business
{
    public class Helper
    {
        string socialDatabaseName;

        string entityDatabaseName;

        public Helper(string socialDatabaseName = null, string entityDatabaseName = null)
        {
            this.socialDatabaseName = socialDatabaseName;
            this.entityDatabaseName = entityDatabaseName;
        }

        public void RemoveEntity(string entityType, Guid entityGuid)
        {
            new LikeBusiness(socialDatabaseName, entityDatabaseName).RemoveLikes(entityType, entityGuid);
            new LikeCountBusiness(socialDatabaseName, entityDatabaseName).RemoveLikeCount(entityType, entityGuid);
            new DislikeBusiness(socialDatabaseName, entityDatabaseName).RemoveDislikes(entityType, entityGuid);
            new DislikeCountBusiness(socialDatabaseName, entityDatabaseName).RemoveDislikeCount(entityType, entityGuid);
            new CommentBusiness(socialDatabaseName, entityDatabaseName).RemoveComments(entityType, entityGuid);
            new CommentCountBusiness(socialDatabaseName, entityDatabaseName).RemoveCommentCount(entityType, entityGuid);
            new ViewBusiness(socialDatabaseName, entityDatabaseName).RemoveViews(entityType, entityGuid);
            new ViewCountBusiness(socialDatabaseName, entityDatabaseName).RemoveViewCount(entityType, entityGuid);
            //new ExcludedEntityBusiness(socialDatabaseName, entityDatabaseName).Include(entityType, entityGuid);
        }

        public void RemoveOrphanEntities(string entityType, List<Guid> entityGuids)
        {
            new LikeBusiness(socialDatabaseName, entityDatabaseName).RemoveOrphanEntities(entityType, entityGuids);
            new LikeCountBusiness(socialDatabaseName, entityDatabaseName).RemoveOrphanEntities(entityType, entityGuids);
            new DislikeBusiness(socialDatabaseName, entityDatabaseName).RemoveOrphanEntities(entityType, entityGuids);
            new DislikeCountBusiness(socialDatabaseName, entityDatabaseName).RemoveOrphanEntities(entityType, entityGuids);
            new CommentBusiness(socialDatabaseName, entityDatabaseName).RemoveOrphanEntities(entityType, entityGuids);
            new CommentCountBusiness(socialDatabaseName, entityDatabaseName).RemoveOrphanEntities(entityType, entityGuids);
            new ViewBusiness(socialDatabaseName, entityDatabaseName).RemoveOrphanEntities(entityType, entityGuids);
            new ViewCountBusiness(socialDatabaseName, entityDatabaseName).RemoveOrphanEntities(entityType, entityGuids);
            //new ExcludedEntityBusiness(socialDatabaseName, entityDatabaseName).RemoveOrphanEntities(entityType, entityGuids);
        }

        public void Inflate(string entityType, object entity,Guid userGuid, params SocialItem[] socialItems)
        {
            foreach (var socialItem in socialItems)
            {
                switch (socialItem)
                {
                    case SocialItem.Like:
                        new LikeBusiness(socialDatabaseName, entityDatabaseName).InflateWithLikesInfo(entityType, entity, userGuid);
                        break;
                    case SocialItem.Dislike:
                        new DislikeBusiness(socialDatabaseName, entityDatabaseName).InflateWithDislikesInfo(entityType, entity, userGuid);
                        break;
                    case SocialItem.View:
                        new ViewBusiness(socialDatabaseName, entityDatabaseName).InflateWithViewsInfo(entityType, entity, userGuid);
                        break;
                    case SocialItem.Comment:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
