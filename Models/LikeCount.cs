namespace Social;

public class LikeCount : IEntity
{
    public LikeCount()
    {
        RelatedItems = new ExpandoObject();
    }

    public long Id { get; set; }

    public Guid EntityTypeGuid { get; set; }

    public Guid EntityGuid { get; set; }

    public long Count { get; set; }

    public dynamic RelatedItems { get; set; }
}
