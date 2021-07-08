using System;

namespace Holism.Social.Models
{
    public class Like : Holism.Models.IEntity
    {
        public Like()
        {
            RelatedItems = new System.Dynamic.ExpandoObject();
        }

        public long Id { get; set; }

        public Guid UserGuid { get; set; }

        public Guid EntityTypeGuid { get; set; }

        public Guid EntityGuid { get; set; }

        public dynamic RelatedItems { get; set; }
    }
}
