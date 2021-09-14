using Holism.DataAccess;
using Holism.Models;
using Holism.Infra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Holism.Social.Business
{
    public interface ISocialBusiness<Entity> where Entity : IGuidEntity, new()
    {
        public string EntityType { get; }

        public Func<List<Guid>> ExcludedEntityGuidsProvider { get; }

        public Func<List<Guid>, List<Entity>> EntityProvider { get; }

        public ListResult<Entity> GetMostLiked(Guid? userGuid, int? pageNumber = null, int? pageSize = null)
        {
            var listParameters = ListParameters.Create(pageNumber, pageSize);
            var mostLikedGuids = new LikeCountBusiness().GetMostLikedGuids(EntityType, listParameters, ExcludedEntityGuidsProvider?.Invoke());
            var mostLikedEntities = mostLikedGuids.BulkConvert(guids => EntityProvider.Invoke(guids));
            mostLikedEntities.Data = KeepOriginalOrder(mostLikedEntities.Data, mostLikedGuids.Data);
            if (userGuid.HasValue)
            {
                new LikeBusiness().InflateWithLikesInfo(EntityType, mostLikedEntities.Data.ToArray(), userGuid.Value);
            }
            return mostLikedEntities;
        }

        public ListResult<Entity> GetMostViewed(Guid? userGuid, int? pageNumber = null, int? pageSize = null)
        {
            var listParameters = ListParameters.Create(pageNumber, pageSize);
            var mostViewedGuids = new ViewCountBusiness().GetMostViewedGuids(EntityType, listParameters, ExcludedEntityGuidsProvider?.Invoke());
            var mostViewedEntities = mostViewedGuids.BulkConvert(guids => EntityProvider.Invoke(guids));
            mostViewedEntities.Data = KeepOriginalOrder(mostViewedEntities.Data, mostViewedGuids.Data);
            if (userGuid.HasValue)
            {
                new LikeBusiness().InflateWithLikesInfo(EntityType, mostViewedEntities.Data.ToArray(), userGuid.Value);
            }
            return mostViewedEntities;
        }

        private List<Entity> KeepOriginalOrder(List<Entity> enttiies, List<Guid> orderedGuids)
        {
            var orderedEntities = enttiies.OrderBy(i => orderedGuids.IndexOf(i.Guid)).ToList();
            return orderedEntities;
        }
    }
}
