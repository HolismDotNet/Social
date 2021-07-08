using Holism.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Holism.Social.DataAccess.DbContexts
{
    public class DislikeDbContext : DbContext
    {
        string databaseName;

        public DislikeDbContext()
            : base()
        {
        }

        public DislikeDbContext(string databaseName)
            : base()
        {
            this.databaseName = databaseName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Config.GetConnectionString(databaseName ?? Config.DatabaseName)).AddInterceptors(new PersianInterceptor());
        }

        public ICollection<Holism.Social.Models.Dislike> Dislikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Holism.Social.Models.Dislike>().ToTable("Dislikes");
            modelBuilder.Entity<Holism.Social.Models.Dislike>().Ignore(i => i.RelatedItems);
            base.OnModelCreating(modelBuilder);
        }
    }
}
