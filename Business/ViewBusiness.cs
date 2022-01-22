namespace Social;

public class ViewBusiness : Business<View, View>
{
    protected override Repository<View> WriteRepository => Repository.View;

    protected override ReadRepository<View> ReadRepository => Repository.View;

    public void RegisterView(Guid userGuid, string entityType, Guid entityGuid)
    {
        var existingView = GetView(userGuid, entityType, entityGuid);
        if (existingView == null)
        {
            View(entityType, userGuid, entityGuid);
        }
        new ViewCountBusiness().IncreaseViewsCount(entityType, entityGuid);
    }

    public object[] InflateWithViewsInfo(string entityType, object[] objects, Guid userGuid)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        if (objects.Length == 0)
        {
            return objects;
        }
        var type = objects.First().GetType();
        var properties = type.GetProperties();
        var entityGuguidProperty = properties.FirstOrDefault(i => i.Name == "Guid");
        var inflatedProperty = properties.FirstOrDefault(i => i.Name == "RelatedItems");
        var entityGuids = objects.Select(i => (Guid)entityGuguidProperty.GetValue(i)).ToList();
        var views = ReadRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && i.UserGuid == userGuid && entityGuids.Contains(i.EntityGuid)).ToList();
        foreach (var @object in objects)
        {
            var view = views.FirstOrDefault(i => i.EntityGuid == (Guid)entityGuguidProperty.GetValue(@object));
            ExpandoObject expando = (ExpandoObject)inflatedProperty.GetValue(@object);
            expando.AddProperty("Viewed", view != null ? true : false);
            inflatedProperty.SetValue(@object, expando);
        }
        return objects;
    }

    public object InflateWithViewsInfo(string entityType, object @object, Guid userGuid)
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
        var view = ReadRepository.All.FirstOrDefault(i => i.EntityTypeGuid == entityTypeGuid && i.UserGuid == userGuid && i.EntityGuid == entityGuid);
        ExpandoObject expando = (ExpandoObject)relatedItemsProperty.GetValue(@object);
        expando.AddProperty("Viewed", view != null ? true : false);
        relatedItemsProperty.SetValue(@object, expando);
        return @object;
    }

    private void View(string entityType, Guid userGuid, Guid entityGuid)
    {
        var existingView = GetView(userGuid, entityType, entityGuid);
        if (existingView != null)
        {
            return;
        }
        var view = new View();
        view.EntityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        view.EntityGuid = entityGuid;
        view.UserGuid = userGuid;
        WriteRepository.Create(view);
        new ViewCountBusiness().IncreaseViewsCount(entityType, entityGuid);
    }

    private View GetView(Guid userGuid, string entityType, Guid entityGuid)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var view = ReadRepository.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid && i.UserGuid == userGuid);
        return view;
    }

    public ListResult<View> GetViewedItems(Guid userGuid, string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
    {
        Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
        var viewdItems = ReadRepository.All.Where(i => i.UserGuid == userGuid && i.EntityTypeGuid == entityTypeGuid).Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListParametersAndGetTotalCount(listParameters);
        return viewdItems;
    }

    public ListResult<Guid> GetViewedItemGuids(Guid userGuid, string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
    {
        var viewdItemGuids = GetViewedItems(userGuid, entityType, listParameters, excludedEntityGuids).BulkConvert<View, Guid>(i => i.Select(x => x.EntityGuid).ToList());
        return viewdItemGuids;
    }

    public void RemoveViews(string entityType, Guid entityGuid)
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
            WriteRepository.Delete(orphanRecord);
        }
    }
}
