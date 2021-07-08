using Holism.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Holism.Social.Models;

namespace Holism.Social.DataAccess
{
    public class SocialContext : DatabaseContext
    {
        public override string ConnectionStringName => "Social";

        public DbSet<Comment> Comments { get; set; }
        
        public DbSet<CommentCount> CommentCounts { get; set; }

        public DbSet<Dislike> Dislikes { get; set; }

        public DbSet<DislikeCount> DislikeCounts { get; set; }

        public DbSet<Like> Likes { get; set; }

        public DbSet<LikeCount> LikeCounts { get; set; }

        public DbSet<View> Views { get; set; }

        public DbSet<ViewCount> ViewCounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
