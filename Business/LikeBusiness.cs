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
    public class LikeBusiness : Business<Like, Like>
    {
        protected override Repository<Like> ModelRepository => RepositoryFactory.LikeFrom(socialDatabaseName);

        protected override ViewRepository<Like> ViewRepository => RepositoryFactory.LikeFrom(socialDatabaseName);

        string socialDatabaseName;

        string entityDatabaseName;

        public LikeBusiness(string socialDatabaseName = null, string entityDatabaseName = null)
        {
            this.socialDatabaseName = socialDatabaseName;
            this.entityDatabaseName = entityDatabaseName;
        }

        public void ToggleLike(Guid userGuid, string entityType, Guid entityGuid)
        {
            var existingLike = GetLike(userGuid, entityType, entityGuid);
            if (existingLike.IsNull())
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
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            if (objects.Length == 0)
            {
                return objects;
            }
            var type = objects.First().GetType();
            var properties = type.GetProperties();
            var guidProperty = properties.FirstOrDefault(i => i.Name == "Guid");
            var inflatedProperty = properties.FirstOrDefault(i => i.Name == "RelatedItems");
            var entityGuids = objects.Select(i => (Guid)guidProperty.GetValue(i)).ToList();
            var likes = ViewRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && i.UserGuid == userGuid && entityGuids.Contains(i.EntityGuid)).ToList();
            foreach (var @object in objects)
            {
                var like = likes.FirstOrDefault(i => i.EntityGuid == (Guid)guidProperty.GetValue(@object));
                ExpandoObject expando = (ExpandoObject)inflatedProperty.GetValue(@object);
                expando.AddProperty("Liked", like.IsNotNull() ? true : false);
                inflatedProperty.SetValue(@object, expando);
            }
            return objects;
        }

        public object InflateWithLikesInfo(string entityType, object @object, Guid userGuid)
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
            var like = ViewRepository.All.FirstOrDefault(i => i.EntityTypeGuid == entityTypeGuid && i.UserGuid == userGuid && i.EntityGuid == entityGuid);
            ExpandoObject expando = (ExpandoObject)relatedItemsProperty.GetValue(@object);
            expando.AddProperty("Liked", like.IsNotNull() ? true : false);
            relatedItemsProperty.SetValue(@object, expando);
            return @object;
        }

        private void Like(string entityType, Guid userGuid, Guid entityGuid)
        {
            var existingLike = GetLike(userGuid, entityType, entityGuid);
            if (existingLike.IsNotNull())
            {
                return;
            }
            var like = new Like();
            like.EntityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            like.EntityGuid = entityGuid;
            like.UserGuid = userGuid;
            ModelRepository.Create(like);
            new LikeCountBusiness(socialDatabaseName, entityDatabaseName).IncreaseLikesCount(entityType, entityGuid);
            new DislikeBusiness(socialDatabaseName, entityDatabaseName).RemoveDislike(entityType, userGuid, entityGuid);
        }

        public void RemoveLike(string entityType, Guid userGuid, Guid entityGuid)
        {
            var existingLike = GetLike(userGuid, entityType, entityGuid);
            if (existingLike.IsNull())
            {
                return;
            }
            ModelRepository.Delete(existingLike);
            new LikeCountBusiness(socialDatabaseName, entityDatabaseName).DecreaseLikesCount(entityType, entityGuid);
        }

        private Like GetLike(Guid userGuid, string entityType, Guid entityGuid)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var like = ViewRepository.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid && i.UserGuid == userGuid);
            return like;
        }

        public ListResult<Like> GetLikedItems(Guid userGuid, string entityType, ListOptions listOptions, List<Guid> excludedEntityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness(entityDatabaseName).GetGuid(entityType);
            var likedItems = ViewRepository.All.Where(i => i.UserGuid == userGuid && i.EntityTypeGuid == entityTypeGuid).Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListOptionsAndGetTotalCount(listOptions);
            return likedItems;
        }

        public ListResult<Guid> GetLikedItemGuids(Guid userGuid, string entityType, ListOptions listOptions, List<Guid> excludedEntityGuids)
        {
            var likedItemGuids = GetLikedItems(userGuid, entityType, listOptions, excludedEntityGuids).BulkConvert<Like, Guid>(i => i.Select(x => x.EntityGuid).ToList());
            return likedItemGuids;
        }

        public void RemoveLikes(string entityType, Guid entityGuid)
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
