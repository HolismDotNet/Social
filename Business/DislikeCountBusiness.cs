namespace Social;

public class DislikeCountBusiness : Business<DislikeCount, DislikeCount>
{
    protected override Repository<DislikeCount> WriteRepository => Repository.DislikeCount;

    protected override ReadRepository<DislikeCount> ReadRepository => Repository.DislikeCount;

    private const string DislikesCountPropertyName = "DislikesCount";

    private DislikeCount GetDislikeCount(string entityType, Guid entityGuid)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var likeCount = ReadRepository.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
        return likeCount;
    }

    public Dictionary<Guid, long> GetDislikeCounts(string entityType, List<Guid> entityGuids)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var likeCounts = ReadRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && entityGuids.Contains(i.EntityGuid)).ToList();
        var result = likeCounts.ToDictionary(i => i.EntityGuid, i => i.Count);
        return result;
    }

    public long GetDislikeCounts(string entityType, Guid entityGuid)
    {
        var likeCounts = GetDislikeCount(entityType, entityGuid);
        if (likeCounts == null)
        {
            return 0;
        }
        return likeCounts.Count;
    }

    public ListResult<DislikeCount> GetMostDisliked(string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        listParameters.AddFilter<DislikeCount>(i => i.EntityTypeGuid, entityTypeGuid.ToString());
        listParameters.AddSort<DislikeCount>(i => i.Count, SortDirection.Descending);
        var likeCounts = ReadRepository.All.Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListParametersAndGetTotalCount(listParameters);
        return likeCounts;
    }

    public ListResult<Guid> GetMostDislikedGuids(string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
    {
        var mostDisliked = GetMostDisliked(entityType, listParameters, excludedEntityGuids);
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
        if (entity == null)
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
        if (likeCount == null)
        {
            likeCount = new DislikeCount();
            likeCount.EntityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            likeCount.EntityGuid = entityGuid;
            likeCount.Count = 1;
            WriteRepository.Create(likeCount);
            return;
        }
        likeCount.Count += 1;
        WriteRepository.Update(likeCount);
    }

    public void DecreaseDislikesCount(string entityType, Guid entityGuid)
    {
        var likeCount = GetDislikeCount(entityType, entityGuid);
        if (likeCount == null)
        {
            return;
        }
        likeCount.Count -= 1;
        if (likeCount.Count < 1)
        {
            WriteRepository.Delete(likeCount);
        }
        else
        {
            WriteRepository.Update(likeCount);
        }
    }

    public void RemoveDislikeCount(string entityType, Guid entityGuid)
    {
        var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var dislikeCount = GetOrNull(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
        WriteRepository.Delete(dislikeCount);
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
