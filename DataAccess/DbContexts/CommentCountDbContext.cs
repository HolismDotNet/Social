using Holism.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Holism.Social.DataAccess.DbContexts
{
    public class CommentCountDbContext : DbContext
    {
        string databaseName;

        public CommentCountDbContext()
            : base()
        {
        }

        public CommentCountDbContext(string databaseName)
            : base()
        {
            this.databaseName = databaseName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Config.GetConnectionString(databaseName ?? Config.DatabaseName)).AddInterceptors(new PersianInterceptor());
        }

        public ICollection<Holism.Social.Models.CommentCount> CommentCounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Holism.Social.Models.CommentCount>().ToTable("CommentCounts");
            modelBuilder.Entity<Holism.Social.Models.CommentCount>().Ignore(i => i.RelatedItems);
            base.OnModelCreating(modelBuilder);
        }
    }
}
