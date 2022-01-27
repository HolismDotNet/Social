namespace Social;

public class ViewCount : IEntity
{
    public ViewCount()
    {
        RelatedItems = new ExpandoObject();
    }

    public long Id { get; set; }

    public Guid EntityTypeGuid { get; set; }

    public Guid EntityGuid { get; set; }

    public long Count { get; set; }

    public dynamic RelatedItems { get; set; }
}
