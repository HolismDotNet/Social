namespace Social;

public class Dislike : IEntity
{
    public Dislike()
    {
        RelatedItems = new ExpandoObject();
    }

    public long Id { get; set; }

    public Guid UserGuid { get; set; }

    public Guid EntityTypeGuid { get; set; }

    public Guid EntityGuid { get; set; }

    public dynamic RelatedItems { get; set; }
}
