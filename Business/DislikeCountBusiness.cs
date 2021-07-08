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
using System.Text;

namespace Holism.Social.Business
{
    public class DislikeCountBusiness : Business<DislikeCount, DislikeCount>
    {
        protected override Repository<DislikeCount> ModelRepository => RepositoryFactory.DislikeCountFrom(socialDatabaseName);

        protected override ViewRepository<DislikeCount> ViewRepository => RepositoryFactory.DislikeCountFrom(socialDatabaseName);

        private const string DislikesCountPropertyName = "DislikesCount";

        string socialDatabaseName;

        string entityDatabaseName;

        public DislikeCountBusiness(string socialDatabaseName = null, string entityDatabaseName = null)
        {
            this.socialDatabaseName = socialDatabaseName;
            this.entityDatabaseName = entityDatabaseName;
        }

        private DislikeCount GetDislikeCount(string entityType, Guid entityGuid)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var likeCount = ViewRepository.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
            return likeCount;
        }

        public Dictionary<Guid, long> GetDislikeCounts(string entityType, List<Guid> entityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var likeCounts = ViewRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && entityGuids.Contains(i.EntityGuid)).ToList();
            var result = likeCounts.ToDictionary(i => i.EntityGuid, i => i.Count);
            return result;
        }

        public long GetDislikeCounts(string entityType, Guid entityGuid)
        {
            var likeCounts = GetDislikeCount(entityType, entityGuid);
            if (likeCounts.IsNull())
            {
                return 0;
            }
            return likeCounts.Count;
        }

        public ListResult<DislikeCount> GetMostDisliked(string entityType, ListOptions listOptions, List<Guid> excludedEntityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            listOptions.AddFilter<DislikeCount>(i => i.EntityTypeGuid, entityTypeGuid.ToString());
            listOptions.AddSort<DislikeCount>(i => i.Count, SortDirection.Descending);
            var likeCounts = ViewRepository.All.Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListOptionsAndGetTotalCount(listOptions);
            return likeCounts;
        }

        public ListResult<Guid> GetMostDislikedGuids(string entityType, ListOptions listOptions, List<Guid> excludedEntityGuids)
        {
            var mostDisliked = GetMostDisliked(entityType, listOptions, excludedEntityGuids);
            var mostDislikedGuids = mostDisliked.Convert(i => i.EntityGuid);
            return mostDislikedGuids;
        }

        public void InflateWithDislikesCount(string entityType, object[] entities)
        {
            if (entities.Length == 0)
            {
                return;
            }
            var guidProperty = entities.First().GetType().GetProperty("Guid");
            var relatedItemsProperty = entities.First().GetType().GetProperty("RelatedItems");
            var guids = entities.Select(i => (Guid)guidProperty.GetValue(i)).ToList();
            var likeCounts = GetDislikeCounts(entityType, guids);
            foreach (var entity in entities)
            {
                if (likeCounts.ContainsKey((Guid)guidProperty.GetValue(entity)))
                {
                    ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), DislikesCountPropertyName, likeCounts[(Guid)guidProperty.GetValue(entity)]);
                }
                else
                {
                    ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), DislikesCountPropertyName, 0);
                }
            }
        }

        public void InflateWithDislikesCount(string entityType, object entity)
        {
            if (entity.IsNull())
            {
                return;
            }
            var guidProperty = entity.GetType().GetProperty("Guid");
            var relatedItemsProperty = entity.GetType().GetProperty("RelatedItems");
            if (ExpandoObjectExtensions.Has((dynamic)relatedItemsProperty.GetValue(entity), DislikesCountPropertyName))
            {
                return;
            }
            var guid = (Guid)guidProperty.GetValue(entity);
            var likeCounts = GetDislikeCounts(entityType, guid);
            ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), DislikesCountPropertyName, likeCounts);
        }

        public void IncreaseDislikesCount(string entityType, Guid entityGuid)
        {
            var likeCount = GetDislikeCount(entityType, entityGuid);
            if (likeCount.IsNull())
            {
                likeCount = new DislikeCount();
                likeCount.EntityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
                likeCount.EntityGuid = entityGuid;
                likeCount.Count = 1;
                ModelRepository.Create(likeCount);
                return;
            }
            likeCount.Count += 1;
            ModelRepository.Update(likeCount);
        }

        public void DecreaseDislikesCount(string entityType, Guid entityGuid)
        {
            var likeCount = GetDislikeCount(entityType, entityGuid);
            if (likeCount.IsNull())
            {
                return;
            }
            likeCount.Count -= 1;
            if (likeCount.Count < 1)
            {
                ModelRepository.Delete(likeCount);
            }
            else
            {
                ModelRepository.Update(likeCount);
            }
        }

        public void RemoveDislikeCount(string entityType, Guid entityGuid)
        {
            var entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var dislikeCount = GetOrNull(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
            ModelRepository.Delete(dislikeCount);
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