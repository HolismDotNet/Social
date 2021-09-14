﻿using Holism.Business;
using Holism.Entity.Business;
using Holism.DataAccess;
using Holism.Infra;
using Holism.Social.DataAccess;
using Holism.Social.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Holism.Social.Business
{
    public class DislikeBusiness : Business<Dislike, Dislike>
    {
        protected override Repository<Dislike> WriteRepository => Repository.Dislike;

        protected override ReadRepository<Dislike> ReadRepository => Repository.Dislike;

        public void ToggleDislike(Guid userGuid, string entityType, Guid entityGuid)
        {
            var existingDislike = GetDislike(userGuid, entityType, entityGuid);
            if (existingDislike == null)
            {
                Dislike(entityType, userGuid, entityGuid);
            }
            else
            {
                RemoveDislike(entityType, userGuid, entityGuid);
            }
        }

        public object[] InflateWithDislikesInfo(string entityType, object[] objects, Guid userGuid)
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
            var dislikes = ReadRepository.All.Where(i => i.EntityTypeGuid == entityTypeGuid && i.UserGuid == userGuid && entityGuids.Contains(i.EntityGuid)).ToList();
            foreach (var @object in objects)
            {
                var dislike = dislikes.FirstOrDefault(i => i.EntityGuid == (Guid)guidProperty.GetValue(@object));
                ExpandoObject expando = (ExpandoObject)inflatedProperty.GetValue(@object);
                expando.AddProperty("Disliked", dislike != null ? true : false);
                inflatedProperty.SetValue(@object, expando);
            }
            return objects;
        }

        public object InflateWithDislikesInfo(string entityType, object @object, Guid userGuid)
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
            var like = ReadRepository.All.FirstOrDefault(i => i.EntityTypeGuid == entityTypeGuid && i.UserGuid == userGuid && i.EntityGuid == entityGuid);
            ExpandoObject expando = (ExpandoObject)relatedItemsProperty.GetValue(@object);
            expando.AddProperty("Disliked", like != null ? true : false);
            relatedItemsProperty.SetValue(@object, expando);
            return @object;
        }

        private void Dislike(string entityType, Guid userGuid, Guid entityGuid)
        {
            var existingDislike = GetDislike(userGuid, entityType, entityGuid);
            if (existingDislike != null)
            {
                return;
            }
            var like = new Dislike();
            like.EntityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            like.EntityGuid = entityGuid;
            like.UserGuid = userGuid;
            WriteRepository.Create(like);
            new DislikeCountBusiness().IncreaseDislikesCount(entityType, entityGuid);
            new LikeBusiness().RemoveLike(entityType, userGuid, entityGuid);
        }

        public void RemoveDislike(string entityType, Guid userGuid, Guid entityGuid)
        {
            var existingDislike = GetDislike(userGuid, entityType, entityGuid);
            if (existingDislike == null)
            {
                return;
            }
            WriteRepository.Delete(existingDislike);
            new DislikeCountBusiness().DecreaseDislikesCount(entityType, entityGuid);
        }

        private Dislike GetDislike(Guid userGuid, string entityType, Guid entityGuid)
        {
            Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            var like = ReadRepository.Get(i => i.EntityTypeGuid == entityTypeGuid && i.EntityGuid == entityGuid && i.UserGuid == userGuid);
            return like;
        }

        public ListResult<Dislike> GetDislikedItems(Guid userGuid, string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
        {
            Guid entityTypeGuid = new EntityTypeBusiness().GetGuid(entityType);
            var likedItems = ReadRepository.All.Where(i => i.UserGuid == userGuid && i.EntityTypeGuid == entityTypeGuid).Where(i => !excludedEntityGuids.Contains(i.EntityGuid)).ApplyListParametersAndGetTotalCount(listParameters);
            return likedItems;
        }

        public ListResult<Guid> GetDislikedItemGuids(Guid userGuid, string entityType, ListParameters listParameters, List<Guid> excludedEntityGuids)
        {
            var likedItemGuids = GetDislikedItems(userGuid, entityType, listParameters, excludedEntityGuids).BulkConvert<Dislike, Guid>(i => i.Select(x => x.EntityGuid).ToList());
            return likedItemGuids;
        }

        public void RemoveDislikes(string entityType, Guid entityGuid)
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
}
