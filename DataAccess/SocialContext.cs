namespace Social;

public class SocialContext : DatabaseContext
{
    public override string ConnectionStringName => "Social";

    public DbSet<Social.CommentCount> CommentCounts { get; set; }

    public DbSet<Social.Comment> Comments { get; set; }

    public DbSet<Social.DislikeCount> DislikeCounts { get; set; }

    public DbSet<Social.Dislike> Dislikes { get; set; }

    public DbSet<Social.LikeCount> LikeCounts { get; set; }

    public DbSet<Social.Like> Likes { get; set; }

    public DbSet<Social.ViewCount> ViewCounts { get; set; }

    public DbSet<Social.View> Views { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
