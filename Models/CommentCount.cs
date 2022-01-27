namespace Social;

public class CommentCount : IEntity
{
    public CommentCount()
    {
        RelatedItems = new ExpandoObject();
    }

    public long Id { get; set; }

    public Guid EntityTypeGuid { get; set; }

    public Guid EntityGuid { get; set; }

    public string Count { get; set; }

    public dynamic RelatedItems { get; set; }
}
