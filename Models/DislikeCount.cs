namespace Social;

public class DislikeCount : IEntity
{
    public DislikeCount()
    {
        RelatedItems = new ExpandoObject();
    }

    public long Id { get; set; }

    public Guid EntityTypeGuid { get; set; }

    public Guid EntityGuid { get; set; }

    public long Count { get; set; }

    public dynamic RelatedItems { get; set; }
}
