using System;

namespace Holism.Social.Models
{
    public class CommentCount : Holism.Models.IEntity
    {
        public CommentCount()
        {
            RelatedItems = new System.Dynamic.ExpandoObject();
        }

        public long Id { get; set; }

        public Guid EntityTypeGuid { get; set; }

        public Guid EntityGuid { get; set; }

        public long Count { get; set; }

        public dynamic RelatedItems { get; set; }
    }
}
