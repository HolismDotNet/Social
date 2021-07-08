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
    public class ViewCountBusiness : Business<ViewCount, ViewCount>
    {
        protected override Repository<ViewCount> ModelRepository => RepositoryFactory.ViewCountFrom(socialDatabaseName);

        protected override ViewRepository<ViewCount> ViewRepository => RepositoryFactory.ViewCountFrom(socialDatabaseName);

        private const string ViewsCountPropertyName = "ViewsCount";

        string socialDatabaseName;

        string entityDatabaseName;

        public ViewCountBusiness(string socialDatabaseName = null, string entityDatabaseName = null)
        {
            this.socialDatabaseName = socialDatabaseName;
            this.entityDatabaseName = entityDatabaseName;
        }

        private ViewCount GetViewCount(string entityType, Guid entityGuid)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var viewCounts = ViewRepository.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
            return viewCounts;
        }

        public Dictionary<Guid, long> GetViewCounts(string entityType, List<Guid> entityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var viewCounts = ViewRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && entityGuids.Contains(i.EntityGuid)).ToList();
            var result = viewCounts.ToDictionary(i => i.EntityGuid, i => i.Count);
            return result;
        }

        public long GetViewCounts(string entityType, Guid entityGuid)
        {
            var viewCounts = GetViewCount(entityType, entityGuid);
            if (viewCounts.IsNull())
            {
                return 0;
            }
            return viewCounts.Count;
        }

        public ListResult<ViewCount> GetMostViewed(string entityType, ListOptions listOptions, List<Guid> excludedEntityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            listOptions.AddFilter<ViewCount>(i => i.EntityTypeGuid, entityTypeGuid.ToString());
            listOptions.AddSort<ViewCount>(i => i.Count, SortDirection.Descending);
            var viewCounts = ViewRepository.All.Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListOptionsAndGetTotalCount(listOptions);
            return viewCounts;
        }

        public ListResult<Guid> GetMostViewedGuids(string entityType, ListOptions listOptions, List<Guid> excludedEntityGuids)
        {
            var mostViewed = GetMostViewed(entityType, listOptions, excludedEntityGuids);
            var mostViewedGuids = mostViewed.Convert(i => i.EntityGuid);
            return mostViewedGuids;
        }

        public void InflateWithViewsCount(string entityType, object[] entities)
        {
            if (entities.Length == 0)
            {
                return;
            }
            var guidProperty = entities.First().GetType().GetProperty("Guid");
            var relatedItemsProperty = entities.First().GetType().GetProperty("RelatedItems");
            var guids = entities.Select(i => (Guid)guidProperty.GetValue(i)).ToList();
            var viewCounts = GetViewCounts(entityType, guids);
            foreach (var entity in entities)
            {
                if (viewCounts.ContainsKey((Guid)guidProperty.GetValue(entity)))
                {
                    ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), ViewsCountPropertyName, viewCounts[(Guid)guidProperty.GetValue(entity)]);
                }
                else
                {
                    ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), ViewsCountPropertyName, 0);
                }
            }
        }

        public void InflateWithViewsCount(string entityType, object entity)
        {
            if (entity.IsNull())
            {
                return;
            }
            var guidProperty = entity.GetType().GetProperty("Guid");
            var relatedItemsProperty = entity.GetType().GetProperty("RelatedItems");
            if (ExpandoObjectExtensions.Has((dynamic)relatedItemsProperty.GetValue(entity), ViewsCountPropertyName))
            {
                return;
            }
            var guid = (Guid)guidProperty.GetValue(entity);
            var viewCounts = GetViewCounts(entityType, guid);
            ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), ViewsCountPropertyName, viewCounts);
        }

        public void IncreaseViewsCount(string entityType, Guid entityGuid)
        {
            var viewCount = GetViewCount(entityType, entityGuid);
            if (viewCount.IsNull())
            {
                viewCount = new ViewCount();
                viewCount.EntityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
                viewCount.EntityGuid = entityGuid;
                viewCount.Count = 1;
                ModelRepository.Create(viewCount);
                return;
            }
            viewCount.Count += 1;
            ModelRepository.Update(viewCount);
        }

        public void RemoveViewCount(string entityType, Guid entityGuid)
        {
            var entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var viewCount = GetOrNull(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
            ModelRepository.Delete(viewCount);
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
