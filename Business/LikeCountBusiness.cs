namespace Social;

public class LikeCountBusiness : Business<LikeCount, LikeCount>
{
    protected override Write<LikeCount> Write => Repository.LikeCount;

    protected override Read<LikeCount> Read => Repository.LikeCount;

    private const string LikesCountPropertyName = "LikesCount";

    private LikeCount GetLikeCount(string entityType, Guid entityGuid)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var likeCount = Read.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
        return likeCount;
    }

    public Dictionary<Guid, long> GetLikeCounts(string entityType, List<Guid> entityGuids)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var likeCounts = Read.All.Where(i => i.EntityTypeGuid == entityTypeGuid && entityGuids.Contains(i.EntityGuid)).ToList();
        var result = likeCounts.ToDictionary(i => i.EntityGuid, i => i.Count);
        return result;
    }

    public long GetLikeCounts(string entityType, Guid entityGuid)
    {
        var likeCounts = GetLikeCount(entityType, entityGuid);
        if (likeCounts == null)
        {
            return 0;
        }
        return likeCounts.Count;
    }

    public ListResult<LikeCount> GetMostLiked(string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        listParameters.AddFilter<LikeCount>(i => i.EntityTypeGuid, entityTypeGuid.ToString());
        listParameters.AddSort<LikeCount>(i => i.Count, SortDirection.Descending);
        var likeCounts = Read.All.Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListParametersAndGetTotalCount(listParameters);
        return likeCounts;
    }

    public ListResult<Guid> GetMostLikedGuids(string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
    {
        var mostLiked = GetMostLiked(entityType, listParameters, excludedEntityGuids);
        var mostLikedGuids = mostLiked.Convert(i => i.EntityGuid);
        return mostLikedGuids;
    }

    public void InflateWithLikesCount(string entityType, object[] entities)
    {
        if (entities.Length == 0)
        {
            return;
        }
        var guguidProperty = entities.First().GetType().GetProperty("Guid");
        var relatedItemsProperty = entities.First().GetType().GetProperty("RelatedItems");
        var ids = entities.Select(i => (Guid)guguidProperty.GetValue(i)).ToList();
        var likeCounts = GetLikeCounts(entityType, ids);
        foreach (var entity in entities)
        {
            if (likeCounts.ContainsKey((Guid)guguidProperty.GetValue(entity)))
            {
                ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), LikesCountPropertyName, likeCounts[(Guid)guguidProperty.GetValue(entity)]);
            }
            else
            {
                ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), LikesCountPropertyName, 0);
            }
        }
    }

    public void InflateWithLikesCount(string entityType, object entity)
    {
        if (entity == null)
        {
            return;
        }
        var guguidProperty = entity.GetType().GetProperty("Guid");
        var relatedItemsProperty = entity.GetType().GetProperty("RelatedItems");
        if (ExpandoObjectExtensions.Has((dynamic)relatedItemsProperty.GetValue(entity), LikesCountPropertyName))
        {
            return;
        }
        var id = (Guid)guguidProperty.GetValue(entity);
        var likeCounts = GetLikeCounts(entityType, id);
        ExpandoObjectExtensions.AddProperty((dynamic)relatedItemsProperty.GetValue(entity), LikesCountPropertyName, likeCounts);
    }

    public void IncreaseLikesCount(string entityType, Guid entityGuid)
    {
        var likeCount = GetLikeCount(entityType, entityGuid);
        if (likeCount == null)
        {
            likeCount = new LikeCount();
            likeCount.EntityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            likeCount.EntityGuid = entityGuid;
            likeCount.Count = 1;
            Write.Create(likeCount);
            return;
        }
        likeCount.Count += 1;
        Write.Update(likeCount);
    }

    public void DecreaseLikesCount(string entityType, Guid entityGuid)
    {
        var likeCount = GetLikeCount(entityType, entityGuid);
        if (likeCount == null)
        {
            return;
        }
        likeCount.Count -= 1;
        if (likeCount.Count < 1)
        {
            Write.Delete(likeCount);
        }
        else
        {
            Write.Update(likeCount);
        }
    }

    public void RemoveLikeCount(string entityType, Guid entityGuid)
    {
        var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var likeCount = Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid);
        Write.Delete(likeCount);
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
