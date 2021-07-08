using System;

namespace Holism.Social.Models
{
    public class Comment : Holism.Models.IEntity
    {
        public Comment()
        {
            RelatedItems = new System.Dynamic.ExpandoObject();
        }

        public long Id { get; set; }

        public Guid UserGuid { get; set; }

        public DateTime Date { get; set; }

        public string PersianDate { get; private set; }

        public Guid EntityTypeGuid { get; set; }

        public Guid EntityGuid { get; set; }

        public string Body { get; set; }

        public bool IsApproved { get; set; }

        public dynamic RelatedItems { get; set; }
    }
}
