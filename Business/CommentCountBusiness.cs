using Holism.Business;
using Holism.Entity.Business;
using Holism.DataAccess;
using Holism.Framework;
using Holism.Framework.Extensions;
using Holism.Social.DataAccess;
using Holism.Social.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Holism.Social.Business
{
    public class CommentCountBusiness : Business<CommentCount, CommentCount>
    {
        protected override Repository<CommentCount> ModelRepository => RepositoryFactory.CommentCountFrom(socialDatabaseName);

        protected override ViewRepository<CommentCount> ViewRepository => RepositoryFactory.CommentCountFrom(socialDatabaseName);

        private const string CommentsCountPropertyName = "CommentsCount";

        string socialDatabaseName;

        string entityDatabaseName;

        public CommentCountBusiness(string socialDatabaseName = null, string entityDatabaseName = null)
        {
            this.socialDatabaseName = socialDatabaseName;
            this.entityDatabaseName = entityDatabaseName;
        }

        private CommentCount GetCommentCount(string entityType, Guid entityGuid)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var commentCount = ViewRepository.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
            return commentCount;
        }

        public Dictionary<Guid, long> GetCommentCounts(string entityType, List<Guid> entityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var commentCounts = ViewRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && entityGuids.Contains(i.EntityGuid)).ToList();
            var result = commentCounts.ToDictionary(i => i.EntityGuid, i => i.Count);
            return result;
        }

        public long GetCommentCounts(string entityType, Guid entityGuid)
        {
            var commentCounts = GetCommentCount(entityType, entityGuid);
            if (commentCounts.IsNull())
            {
                return 0;
            }
            return commentCounts.Count;
        }

        public ListResult<CommentCount> GetMostCommented(string entityType, ListOptions listOptions, List<Guid> excludedEntityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            listOptions.AddFilter<CommentCount>(i => i.EntityTypeGuid, entityTypeGuid.ToString());
            listOptions.AddSort<CommentCount>(i => i.Count, SortDirection.Descending);
            var commentCounts = ViewRepository.All.Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListOptionsAndGetTotalCount(listOptions);
            return commentCounts;
        }

        public ListResult<Guid> GetMostCommentedGuids(string entityType, ListOptions listOptions, List<Guid> excludedEntityGuids)
        {
            var mostCommented = GetMostCommented(entityType, listOptions, excludedEntityGuids);
            var mostCommentedGuids = mostCommented.Convert(i => i.EntityGuid);
            return mostCommentedGuids;
        }

        public void InflateWithCommentsCount(string entityType, object[] entities)
        {
            if (entities.Length == 0)
            {
                return;
            }
            var guidProperty = entities.First().GetType().GetProperty("Guid");
            var relatedItemsProperty = entities.First().GetType().GetProperty("RelatedItems");
            var guids = entities.Select(i => (Guid)guidProperty.GetValue(i)).ToList();
            var commentCounts = GetCommentCounts(entityType, guids);
            foreach (var entity in entities)
            {
                if (commentCounts.ContainsKey((Guid)guidProperty.GetValue(entity)))
                {
                    ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), CommentsCountPropertyName, commentCounts[(Guid)guidProperty.GetValue(entity)]);
                }
                else
                {
                    ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), CommentsCountPropertyName, 0);
                }
            }
        }

        public void InflateWithCommentsCount(string entityType, object entity)
        {
            if (entity.IsNull())
            {
                return;
            }
            var guidProperty = entity.GetType().GetProperty("Guid");
            var relatedItemsProperty = entity.GetType().GetProperty("RelatedItems");
            if (ExpandoObjectExtensions.Has((dynamic)relatedItemsProperty.GetValue(entity), CommentsCountPropertyName))
            {
                return;
            }
            var guid = (Guid)guidProperty.GetValue(entity);
            var commentCounts = GetCommentCounts(entityType, guid);
            ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), CommentsCountPropertyName, commentCounts);
        }

        public void IncreaseCommentsCount(string entityType, Guid entityGuid)
        {
            var commentCount = GetCommentCount(entityType, entityGuid);
            if (commentCount.IsNull())
            {
                commentCount = new CommentCount();
                commentCount.EntityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
                commentCount.EntityGuid = entityGuid;
                commentCount.Count = 1;
                ModelRepository.Create(commentCount);
                return;
            }
            commentCount.Count += 1;
            ModelRepository.Update(commentCount);
        }

        public void DecreaseCommentsCount(string entityType, Guid entityGuid)
        {
            var commentCount = GetCommentCount(entityType, entityGuid);
            if (commentCount.IsNull())
            {
                return;
            }
            commentCount.Count -= 1;
            if (commentCount.Count < 1)
            {
                ModelRepository.Delete(commentCount);
            }
            else
            {
                ModelRepository.Update(commentCount);
            }
        }

        public void RemoveCommentCount(string entityType, Guid entityGuid)
        {
            var entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var commentCount = GetOrNull(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
            ModelRepository.Delete(commentCount);
        }

        public void RemoveOrphanEntities(string entityType, List<Guid> entityGuids)
        {
            var entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var orphanRecords = ViewRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && !entityGuids.Contains(i.EntityGuid)).ToList();
            foreach (var orphanRecord in orphanRecords)
            {
                ModelRepository.Delete(orphanRecord);
            }
        }
    }
}
