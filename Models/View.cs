namespace Social;

public class View : IEntity
{
    public View()
    {
        RelatedItems = new ExpandoObject();
    }

    public long Id { get; set; }

    public Guid UserGuid { get; set; }

    public Guid EntityTypeGuid { get; set; }

    public Guid EntityGuid { get; set; }

    public dynamic RelatedItems { get; set; }
}
