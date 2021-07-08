using Holism.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Holism.Social.DataAccess.DbContexts
{
    public class DislikeCountDbContext : DbContext
    {
        string databaseName;

        public DislikeCountDbContext()
            : base()
        {
        }

        public DislikeCountDbContext(string databaseName)
            : base()
        {
            this.databaseName = databaseName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Config.GetConnectionString(databaseName ?? Config.DatabaseName)).AddInterceptors(new PersianInterceptor());
        }

        public ICollection<Holism.Social.Models.DislikeCount> DislikeCounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Holism.Social.Models.DislikeCount>().ToTable("DislikeCounts");
            modelBuilder.Entity<Holism.Social.Models.DislikeCount>().Ignore(i => i.RelatedItems);
            base.OnModelCreating(modelBuilder);
        }
    }
}
