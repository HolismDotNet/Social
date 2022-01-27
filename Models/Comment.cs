namespace Social;

public class Comment : IEntity
{
    public Comment()
    {
        RelatedItems = new ExpandoObject();
    }

    public long Id { get; set; }

    public Guid UserGuid { get; set; }

    public DateTime UtcDate { get; set; }

    public Guid EntityTypeGuid { get; set; }

    public Guid EntityGuid { get; set; }

    public string Body { get; set; }

    public bool IsApproved { get; set; }

    public dynamic RelatedItems { get; set; }
}
