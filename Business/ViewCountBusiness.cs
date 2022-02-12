namespace Social;

public class ViewCountBusiness : Business<ViewCount, ViewCount>
{
    protected override Write<ViewCount> Write => Repository.ViewCount;

    protected override Read<ViewCount> Read => Repository.ViewCount;

    private const string ViewsCountPropertyName = "ViewsCount";

    private ViewCount GetViewCount(string entityType, Guid entityGuid)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var viewCounts = Read.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
        return viewCounts;
    }

    public Dictionary<Guid, long> GetViewCounts(string entityType, List<Guid> entityGuids)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var viewCounts = Read.All.Where(i => i.EntityTypeGuid == entityTypeGuid && entityGuids.Contains(i.EntityGuid)).ToList();
        var result = viewCounts.ToDictionary(i => i.EntityGuid, i => i.Count);
        return result;
    }

    public long GetViewCounts(string entityType, Guid entityGuid)
    {
        var viewCounts = GetViewCount(entityType, entityGuid);
        if (viewCounts == null)
        {
            return 0;
        }
        return viewCounts.Count;
    }

    public ListResult<ViewCount> GetMostViewed(string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        listParameters.AddFilter<ViewCount>(i => i.EntityTypeGuid, entityTypeGuid.ToString());
        listParameters.AddSort<ViewCount>(i => i.Count, SortDirection.Descending);
        var viewCounts = Read.All.Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListParametersAndGetTotalCount(listParameters);
        return viewCounts;
    }

    public ListResult<Guid> GetMostViewedGuids(string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
    {
        var mostViewed = GetMostViewed(entityType, listParameters, excludedEntityGuids);
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
        if (entity == null)
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
        if (viewCount == null)
        {
            viewCount = new ViewCount();
            viewCount.EntityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            viewCount.EntityGuid = entityGuid;
            viewCount.Count = 1;
            Write.Create(viewCount);
            return;
        }
        viewCount.Count += 1;
        Write.Update(viewCount);
    }

    public void RemoveViewCount(string entityType, Guid entityGuid)
    {
        var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var viewCount = GetOrNull(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
        Write.Delete(viewCount);
    }

    public void RemoveOrphanEntities(string entityType, List<Guid> entityGuids)
    {
        var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var orphanRecords = Read.All.Where(i => i.EntityTypeGuid == entityTypeGuid && !entityGuids.Contains(i.EntityGuid)).ToList();
        foreach (var orphanRecord in orphanRecords)
        {
            Write.Delete(orphanRecord);
        }
    }
}
