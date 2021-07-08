using Holism.Business;
using Holism.Entity.Business;
using Holism.DataAccess;
using Holism.Framework;
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

        protected override Repository<Comment> WriteRepository => Repository.Comment;

        protected override ReadRepository<Comment> ReadRepository => Repository.Comment;

        private static Dictionary<Guid, Func<List<Guid>, Dictionary<Guid, object>>> entitiesAugmenter = new Dictionary<Guid, Func<List<Guid>, Dictionary<Guid, object>>>();

        public static void RegisterEntityAugmenter(string entityType, Func<List<Guid>, Dictionary<Guid, object>> augmenter)
        {
            var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            if (entitiesAugmenter.ContainsKey(entityTypeGuid))
            {
                entitiesAugmenter[entityTypeGuid] = augmenter;
            }
            else
            {
                entitiesAugmenter.Add(entityTypeGuid, augmenter);
            }
        }

        public override ListResult<Comment> GetList(ListParameters listParameters)
        {
            if (!listParameters.HasSorts)
            {
                listParameters.AddSort<Comment>(i => i.Date, SortDirection.Descending);
            }
            return base.GetList(listParameters);
        }

        public override void Validate(Comment model)
        {
            model.EntityTypeGuid.Ensure().IsNumeric().And().IsGreaterThanZero();
            model.EntityGuid.Ensure().IsNumeric().And().IsGreaterThanZero();
            model.UserGuid.Ensure().IsNumeric("کاربر مشخص نشده است").And().IsGreaterThanZero("کاربر تعیین شده صحیح نیست");
            model.Body.Ensure().IsSomething("کامنت باید حتما متن داشته باشه");
        }

        public void ToggleApprovedState(long id)
        {
            var comment = WriteRepository.Get(id);
            comment.IsApproved = !comment.IsApproved;
            Update(comment);
        }

        public void ApproveItems(List<long> ids)
        {
            var comments = WriteRepository.GetList(ids);
            foreach (var comment in comments)
            {
                comment.IsApproved = true;
            }
            WriteRepository.BulkUpdate(comments);
        }

        public void DisapproveItems(List<long> ids)
        {
            var comments = WriteRepository.GetList(ids);
            foreach (var comment in comments)
            {
                comment.IsApproved = false;
            }
            WriteRepository.BulkUpdate(comments);
        }

        public ListResult<Comment> GetComments(string entityType, Guid entityGuid, int pageNumber)
        {
            var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            var listParameters = ListParameters.Create();
            listParameters.PageNumber = pageNumber;
            listParameters.AddFilter<Comment>(i => i.EntityTypeGuid, entityTypeGuid.ToString());
            listParameters.AddFilter<Comment>(i => i.EntityGuid, entityGuid.ToString());
            listParameters.AddFilter<Comment>(i => i.IsApproved, true.ToString());
            var result = GetList(listParameters);
            return result;
        }

        public Comment Create(Guid userGuid, string entityType, Guid entityGuid, string body)
        {
            var comment = new Comment();
            comment.UserGuid = userGuid;
            comment.EntityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            comment.Body = body;
            comment.Date = DateTime.Now;
            comment.EntityGuid = entityGuid;
            comment.IsApproved = false;
            return Create(comment);
        }

        protected override void ModifyItemBeforeReturning(Comment item)
        {
            new LikeCountBusiness().InflateWithLikesCount(EntityType, item);
            new DislikeCountBusiness().InflateWithDislikesCount(EntityType, item);
            new UserBusiness().InflateWithUsernameAndProfilePictures(item);
            if (!ExpandoObjectExtensions.Has(item.RelatedItems, EntityInfoPropertyName))
            {
                AugmentWithEntitiesInfo(new List<Comment> { item });
            }
            item.RelatedItems.TimeAgo = PersianDateTime.GetTimeAgo(item.Date);
            new EntityTypeBusiness().InflateWithEntityType(item);
        }

        protected override void ModifyListBeforeReturning(List<Comment> items)
        {
            new LikeCountBusiness().InflateWithLikesCount(EntityType, items.ToArray());
            new DislikeCountBusiness().InflateWithDislikesCount(EntityType, items.ToArray());
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
            if (entitiesAugmenter.ContainsKey(entityTypeGuid))
            {
                var entityGuids = list.Select(i => i.EntityGuid).ToList();
                var entityInfoList = entitiesAugmenter[entityTypeGuid](entityGuids);
                var commentsWithEntityInfo = list.Where(i => entityInfoList.ContainsKey(i.EntityGuid)).ToList();
                foreach (var comment in commentsWithEntityInfo)
                {
                    ExpandoObjectExtensions.AddProperty(comment.RelatedItems, EntityInfoPropertyName, entityInfoList[comment.EntityGuid]);
                }
            }
        }

        public void RemoveComments(string entityType, Guid entityGuid)
        {
            var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            var query = $@"
delete
from {WriteRepository.TableName}
where EntityTypeGuid = '{entityTypeGuid}'
and EntityGuid = '{entityGuid}'
            ";
            WriteRepository.Run(query);
        }

        public void RemoveOrphanEntities(string entityType, List<Guid> entityGuids)
        {
            var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            var orphanRecords = ReadRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && !entityGuids.Contains(i.EntityGuid)).ToList();
            foreach (var orphanRecord in orphanRecords)
            {
                WriteRepository.Delete(orphanRecord.Id);
            }
        }
    }
}
