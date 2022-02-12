namespace Social;

public class LikeBusiness : Business<Like, Like>
{
    protected override Write<Like> Write => Repository.Like;

    protected override Read<Like> Read => Repository.Like;

    public void ToggleLike(Guid userGuid, string entityType, Guid entityGuid)
    {
        var existingLike = GetLike(userGuid, entityType, entityGuid);
        if (existingLike == null)
        {
            Like(entityType, userGuid, entityGuid);
        }
        else
        {
            RemoveLike(entityType, userGuid, entityGuid);
        }
    }

    public object[] InflateWithLikesInfo(string entityType, object[] objects, Guid userGuid)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        if (objects.Length == 0)
        {
            return objects;
        }
        var type = objects.First().GetType();
        var properties = type.GetProperties();
        var guidProperty = properties.FirstOrDefault(i => i.Name == "Guid");
        var inflatedProperty = properties.FirstOrDefault(i => i.Name == "RelatedItems");
        var entityGuids = objects.Select(i => (Guid)guidProperty.GetValue(i)).ToList();
        var likes = Read.All.Where(i => i.EntityTypeGuid == entityTypeGuid && i.UserGuid == userGuid && entityGuids.Contains(i.EntityGuid)).ToList();
        foreach (var @object in objects)
        {
            var like = likes.FirstOrDefault(i => i.EntityGuid == (Guid)guidProperty.GetValue(@object));
            ExpandoObject expando = (ExpandoObject)inflatedProperty.GetValue(@object);
            expando.AddProperty("Liked", like != null ? true : false);
            inflatedProperty.SetValue(@object, expando);
        }
        return objects;
    }

    public object InflateWithLikesInfo(string entityType, object @object, Guid userGuid)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        if (@object == null)
        {
            return @object;
        }
        var type = @object.GetType();
        var properties = type.GetProperties();
        var guidProperty = properties.FirstOrDefault(i => i.Name == "Guid");
        var relatedItemsProperty = properties.FirstOrDefault(i => i.Name == "RelatedItems");
        var entityGuid = (Guid)guidProperty.GetValue(@object);
        var like = Read.All.FirstOrDefault(i => i.EntityTypeGuid == entityTypeGuid && i.UserGuid == userGuid && i.EntityGuid == entityGuid);
        ExpandoObject expando = (ExpandoObject)relatedItemsProperty.GetValue(@object);
        expando.AddProperty("Liked", like != null ? true : false);
        relatedItemsProperty.SetValue(@object, expando);
        return @object;
    }

    private void Like(string entityType, Guid userGuid, Guid entityGuid)
    {
        var existingLike = GetLike(userGuid, entityType, entityGuid);
        if (existingLike != null)
        {
            return;
        }
        var like = new Like();
        like.EntityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        like.EntityGuid = entityGuid;
        like.UserGuid = userGuid;
        Write.Create(like);
        new LikeCountBusiness().IncreaseLikesCount(entityType, entityGuid);
        new DislikeBusiness().RemoveDislike(entityType, userGuid, entityGuid);
    }

    public void RemoveLike(string entityType, Guid userGuid, Guid entityGuid)
    {
        var existingLike = GetLike(userGuid, entityType, entityGuid);
        if (existingLike == null)
        {
            return;
        }
        Write.Delete(existingLike);
        new LikeCountBusiness().DecreaseLikesCount(entityType, entityGuid);
    }

    private Like GetLike(Guid userGuid, string entityType, Guid entityGuid)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var like = Read.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid && i.UserGuid == userGuid);
        return like;
    }

    public ListResult<Like> GetLikedItems(Guid userGuid, string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var likedItems = Read.All.Where(i => i.UserGuid == userGuid && i.EntityTypeGuid == entityTypeGuid).Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListParametersAndGetTotalCount(listParameters);
        return likedItems;
    }

    public ListResult<Guid> GetLikedItemGuids(Guid userGuid, string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
    {
        var likedItemGuids = GetLikedItems(userGuid, entityType, listParameters, excludedEntityGuids).BulkConvert<Like, Guid>(i => i.Select(x => x.EntityGuid).ToList());
        return likedItemGuids;
    }

    public void RemoveLikes(string entityType, Guid entityGuid)
    {
        var entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var query = $@"
delete
from {Write.TableName}
where EntityTypeGuid = '{entityTypeGuid}'
and EntityGuid = '{entityGuid}'
        ";
        Write.Run(query);
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
