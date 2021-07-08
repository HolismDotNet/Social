using Holism.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Holism.Social.DataAccess.DbContexts
{
    public class ViewCountDbContext : DbContext
    {
        string databaseName;

        public ViewCountDbContext()
            : base()
        {
        }

        public ViewCountDbContext(string databaseName)
            : base()
        {
            this.databaseName = databaseName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Config.GetConnectionString(databaseName ?? Config.DatabaseName)).AddInterceptors(new PersianInterceptor());
        }

        public ICollection<Holism.Social.Models.ViewCount> ViewCounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Holism.Social.Models.ViewCount>().ToTable("ViewCounts");
            modelBuilder.Entity<Holism.Social.Models.ViewCount>().Ignore(i => i.RelatedItems);
            base.OnModelCreating(modelBuilder);
        }
    }
}
