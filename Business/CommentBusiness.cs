using Holism.Identities.Business;
using Holism.Business;
using Holism.Entity.Business;
using Holism.DataAccess;
using Holism.Framework;
using Holism.Framework.Extensions;
using Holism.Social.DataAccess;
using Holism.Social.Models;
using Holism.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Holism.Social.Business
{
    public class CommentBusiness : Business<Comment, Comment>
    {
        public const string EntityType = "Comment";

        private const string EntityInfoPropertyName = "Entity";

        protected override Repository<Comment> ModelRepository => RepositoryFactory.CommentFrom(socialDatabaseName);

        protected override ViewRepository<Comment> ViewRepository => RepositoryFactory.CommentFrom(socialDatabaseName);

        private static Dictionary<string, Dictionary<Guid, Func<List<Guid>, Dictionary<Guid, object>>>> entitiesInfoAugmenter = new Dictionary<string, Dictionary<Guid, Func<List<Guid>, Dictionary<Guid, object>>>>();

        string socialDatabaseName;

        string entityDatabaseName;

        public CommentBusiness(string socialDatabaseName = null, string entityDatabaseName = null)
        {
            this.socialDatabaseName = socialDatabaseName;
            this.entityDatabaseName = entityDatabaseName;
        }

        public static void RegisterEnittyInfoAugmenter(string entityDatabaseName, string entityType, Func<List<Guid>, Dictionary<Guid, object>> augmenter)
        {
            if (entityDatabaseName.IsNothing())
            {
                throw new FrameworkException($"Database is not specified. To work with entitites, you should specify the requested database.");
            }
            var entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            if (!entitiesInfoAugmenter.ContainsKey(entityDatabaseName))
            {
                entitiesInfoAugmenter.Add(entityDatabaseName, new Dictionary<Guid, Func<List<Guid>, Dictionary<Guid, object>>>());
            }
            if (entitiesInfoAugmenter[entityDatabaseName].ContainsKey(entityTypeGuid))
            {
                entitiesInfoAugmenter[entityDatabaseName][entityTypeGuid] = augmenter;
            }
            else
            {
                entitiesInfoAugmenter[entityDatabaseName].Add(entityTypeGuid, augmenter);
            }
        }

        public override ListResult<Comment> GetList(ListOptions listOptions)
        {
            if (!listOptions.HasSorts)
            {
                listOptions.AddSort<Comment>(i => i.Date, SortDirection.Descending);
            }
            return base.GetList(listOptions);
        }

        public override void Validate(Comment model)
        {
            model.EntityTypeGuid.Ensure().IsNumeric().And().IsGreaterThanZero();
            model.EntityGuid.Ensure().IsNumeric().And().IsGreaterThanZero();
            model.UserGuid.Ensure().IsNumeric("کاربر مشخص نشده است").And().IsGreaterThanZero("کاربر تعیین شده صحیح نیست");
            model.Body.Ensure().AsString().IsSomething("کامنت باید حتما متن داشته باشه");
        }

        public void ToggleApprovedState(long id)
        {
            var comment = ModelRepository.Get(id);
            comment.IsApproved = !comment.IsApproved;
            Update(comment);
        }

        public void ApproveItems(List<long> ids)
        {
            var comments = ModelRepository.GetList(ids);
            foreach (var comment in comments)
            {
                comment.IsApproved = true;
            }
            ModelRepository.BulkUpdate(comments);
        }

        public void DisapproveItems(List<long> ids)
        {
            var comments = ModelRepository.GetList(ids);
            foreach (var comment in comments)
            {
                comment.IsApproved = false;
            }
            ModelRepository.BulkUpdate(comments);
        }

        public ListResult<Comment> GetComments(string entityType, Guid entityGuid, int pageNumber)
        {
            var entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var listOptions = ListOptions.Create();
            listOptions.PageNumber = pageNumber;
            listOptions.AddFilter<Comment>(i => i.EntityTypeGuid, entityTypeGuid.ToString());
            listOptions.AddFilter<Comment>(i => i.EntityGuid, entityGuid.ToString());
            listOptions.AddFilter<Comment>(i => i.IsApproved, true.ToString());
            var result = GetList(listOptions);
            return result;
        }

        public Comment Create(Guid userGuid, string entityType, Guid entityGuid, string body)
        {
            var comment = new Comment();
            comment.UserGuid = userGuid;
            comment.EntityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            comment.Body = body;
            comment.Date = DateTime.Now;
            comment.EntityGuid = entityGuid;
            comment.IsApproved = false;
            return Create(comment);
        }

        protected override void ModifyItemBeforeReturning(Comment item)
        {
            new LikeCountBusiness(socialDatabaseName, entityDatabaseName).InflateWithLikesCount(EntityType, item);
            new DislikeCountBusiness(socialDatabaseName, entityDatabaseName).InflateWithDislikesCount(EntityType, item);
            new UserBusiness().InflateWithUsernameAndProfilePictures(item);
            if (!ExpandoObjectExtensions.Has(item.RelatedItems, EntityInfoPropertyName))
            {
                AugmentWithEntitiesInfo(new List<Comment> { item });
            }
            item.RelatedItems.TimeAgo = PersianDateTime.GetTimeAgo(item.Date);
            new EntityTypeBusiness(entityDatabaseName).InflateWithEntityType(item);
        }

        protected override void ModifyListBeforeReturning(List<Comment> items)
        {
            new LikeCountBusiness(socialDatabaseName, entityDatabaseName).InflateWithLikesCount(EntityType, items.ToArray());
            new DislikeCountBusiness(socialDatabaseName, entityDatabaseName).InflateWithDislikesCount(EntityType, items.ToArray());
            new UserBusiness().InflateWithUsernameAndProfilePictures(items.ToArray());
            AugmentWithEntitiesInfo(items);
            base.ModifyListBeforeReturning(items);
        }

        private void AugmentWithEntitiesInfo(List<Comment> list)
        {
            if (list.Count == 0)
            {
                return;
            }
            var entityTypeGuid = list.First().EntityTypeGuid;
            if (entitiesInfoAugmenter[entityDatabaseName].ContainsKey(entityTypeGuid))
            {
                var entityGuids = list.Select(i => i.EntityGuid).ToList();
                var entityInfoList = entitiesInfoAugmenter[entityDatabaseName][entityTypeGuid](entityGuids);
                var commentsWithEntityInfo = list.Where(i => entityInfoList.ContainsKey(i.EntityGuid)).ToList();
                foreach (var comment in commentsWithEntityInfo)
                {
                    ExpandoObjectExtensions.AddProperty(comment.RelatedItems, EntityInfoPropertyName, entityInfoList[comment.EntityGuid]);
                }
            }
        }

        public void RemoveComments(string entityType, Guid entityGuid)
        {
            var entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var query = $@"
delete
from {ModelRepository.TableName}
where EntityTypeGuid = '{entityTypeGuid}'
and EntityGuid = '{entityGuid}'
            ";
            ModelRepository.Run(query);
        }

        public void RemoveOrphanEntities(string entityType, List<Guid> entityGuids)
        {
            var entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var orphanRecords = ViewRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && !entityGuids.Contains(i.EntityGuid)).ToList();
            foreach (var orphanRecord in orphanRecords)
            {
                ModelRepository.Delete(orphanRecord.Id);
            }
        }
    }
}
