using Holism.Business;
using Holism.Entity.Business;
using Holism.DataAccess;
using Holism.Framework;
using Holism.Social.DataAccess;
using Holism.Social.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Holism.Social.Business
{
    public class CommentCountBusiness : Business<CommentCount, CommentCount>
    {
        protected override Repository<CommentCount> WriteRepository => Repository.CommentCount;

        protected override ReadRepository<CommentCount> ReadRepository => Repository.CommentCount;

        private const string CommentsCountPropertyName = "CommentsCount";

        private CommentCount GetCommentCount(string entityType, Guid entityGuid)
        {
            Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            var commentCount = ReadRepository.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
            return commentCount;
        }

        public Dictionary<Guid, long> GetCommentCounts(string entityType, List<Guid> entityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            var commentCounts = ReadRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && entityGuids.Contains(i.EntityGuid)).ToList();
            var result = commentCounts.ToDictionary(i => i.EntityGuid, i => i.Count);
            return result;
        }

        public long GetCommentCounts(string entityType, Guid entityGuid)
        {
            var commentCounts = GetCommentCount(entityType, entityGuid);
            if (commentCounts == null)
            {
                return 0;
            }
            return commentCounts.Count;
        }

        public ListResult<CommentCount> GetMostCommented(string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            listParameters.AddFilter<CommentCount>(i => i.EntityTypeGuid, entityTypeGuid.ToString());
            listParameters.AddSort<CommentCount>(i => i.Count, SortDirection.Descending);
            var commentCounts = ReadRepository.All.Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListParametersAndGetTotalCount(listParameters);
            return commentCounts;
        }

        public ListResult<Guid> GetMostCommentedGuids(string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
        {
            var mostCommented = GetMostCommented(entityType, listParameters, excludedEntityGuids);
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
            if (entity == null)
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
            if (commentCount == null)
            {
                commentCount = new CommentCount();
                commentCount.EntityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
                commentCount.EntityGuid = entityGuid;
                commentCount.Count = 1;
                WriteRepository.Create(commentCount);
                return;
            }
            commentCount.Count += 1;
            WriteRepository.Update(commentCount);
        }

        public void DecreaseCommentsCount(string entityType, Guid entityGuid)
        {
            var commentCount = GetCommentCount(entityType, entityGuid);
            if (commentCount == null)
            {
                return;
            }
            commentCount.Count -= 1;
            if (commentCount.Count < 1)
            {
                WriteRepository.Delete(commentCount);
            }
            else
            {
                WriteRepository.Update(commentCount);
            }
        }

        public void RemoveCommentCount(string entityType, Guid entityGuid)
        {
            var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            var commentCount = GetOrNull(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
            WriteRepository.Delete(commentCount);
        }

        public void RemoveOrphanEntities(string entityType, List<Guid> entityGuids)
        {
            var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            var orphanRecords = ReadRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && !entityGuids.Contains(i.EntityGuid)).ToList();
            foreach (var orphanRecord in orphanRecords)
            {
                WriteRepository.Delete(orphanRecord);
            }
        }
    }
}
