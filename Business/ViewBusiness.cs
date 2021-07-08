using Holism.Business;
using Holism.Entity.Business;
using Holism.DataAccess;
using Holism.Framework;
using Holism.Framework.Extensions;
using Holism.Social.DataAccess;
using Holism.Social.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Holism.Social.Business
{
    public class ViewBusiness : Business<View, View>
    {
        protected override Repository<View> ModelRepository => RepositoryFactory.ViewFrom(socialDatabaseName);

        protected override ViewRepository<View> ViewRepository => RepositoryFactory.ViewFrom(socialDatabaseName);

        string socialDatabaseName;

        string entityDatabaseName;

        public ViewBusiness(string socialDatabaseName = null, string entityDatabaseName = null)
        {
            this.socialDatabaseName = socialDatabaseName;
            this.entityDatabaseName = entityDatabaseName;
        }

        public void RegisterView(Guid userGuid, string entityType, Guid entityGuid)
        {
            var existingView = GetView(userGuid, entityType, entityGuid);
            if (existingView.IsNull())
            {
                View(entityType, userGuid, entityGuid);
            }
            new ViewCountBusiness(socialDatabaseName, entityDatabaseName).IncreaseViewsCount(entityType, entityGuid);
        }

        public object[] InflateWithViewsInfo(string entityType, object[] objects, Guid userGuid)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            if (objects.Length == 0)
            {
                return objects;
            }
            var type = objects.First().GetType();
            var properties = type.GetProperties();
            var entityGuguidProperty = properties.FirstOrDefault(i => i.Name == "Guid");
            var inflatedProperty = properties.FirstOrDefault(i => i.Name == "RelatedItems");
            var entityGuids = objects.Select(i => (Guid)entityGuguidProperty.GetValue(i)).ToList();
            var views = ViewRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && i.UserGuid == userGuid && entityGuids.Contains(i.EntityGuid)).ToList();
            foreach (var @object in objects)
            {
                var view = views.FirstOrDefault(i => i.EntityGuid == (Guid)entityGuguidProperty.GetValue(@object));
                ExpandoObject expando = (ExpandoObject)inflatedProperty.GetValue(@object);
                expando.AddProperty("Viewed", view.IsNotNull() ? true : false);
                inflatedProperty.SetValue(@object, expando);
            }
            return objects;
        }

        public object InflateWithViewsInfo(string entityType, object @object, Guid userGuid)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            if (@object.IsNull())
            {
                return @object;
            }
            var type = @object.GetType();
            var properties = type.GetProperties();
            var guidProperty = properties.FirstOrDefault(i => i.Name == "Guid");
            var relatedItemsProperty = properties.FirstOrDefault(i => i.Name == "RelatedItems");
            var entityGuid = (Guid)guidProperty.GetValue(@object);
            var view = ViewRepository.All.FirstOrDefault(i => i.EntityTypeGuid == entityTypeGuid && i.UserGuid == userGuid && i.EntityGuid == entityGuid);
            ExpandoObject expando = (ExpandoObject)relatedItemsProperty.GetValue(@object);
            expando.AddProperty("Viewed", view.IsNotNull() ? true : false);
            relatedItemsProperty.SetValue(@object, expando);
            return @object;
        }

        private void View(string entityType, Guid userGuid, Guid entityGuid)
        {
            var existingView = GetView(userGuid, entityType, entityGuid);
            if (existingView.IsNotNull())
            {
                return;
            }
            var view = new View();
            view.EntityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            view.EntityGuid = entityGuid;
            view.UserGuid = userGuid;
            ModelRepository.Create(view);
            new ViewCountBusiness(socialDatabaseName, entityDatabaseName).IncreaseViewsCount(entityType, entityGuid);
        }

        private View GetView(Guid userGuid, string entityType, Guid entityGuid)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var view = ViewRepository.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid && i.UserGuid == userGuid);
            return view;
        }

        public ListResult<View> GetViewedItems(Guid userGuid, string entityType, ListOptions listOptions, List<Guid> excludedEntityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var viewdItems = ViewRepository.All.Where(i => i.UserGuid == userGuid && i.EntityTypeGuid == entityTypeGuid).Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListOptionsAndGetTotalCount(listOptions);
            return viewdItems;
        }

        public ListResult<Guid> GetViewedItemGuids(Guid userGuid, string entityType, ListOptions listOptions, List<Guid> excludedEntityGuids)
        {
            var viewdItemGuids = GetViewedItems(userGuid, entityType, listOptions, excludedEntityGuids).BulkConvert<View, Guid>(i => i.Select(x => x.EntityGuid).ToList());
            return viewdItemGuids;
        }

        public void RemoveViews(string entityType, Guid entityGuid)
        {
            var entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var query = $@"
delete
from {ModelRepository.TableName}
where EntityTypeGuid = '{entityTypeGuid}'
and EntityGuid = '{entityGuid}'
            ";
            ModelRepository.Run(query);
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
