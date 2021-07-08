using Holism.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Holism.Social.DataAccess.DbContexts
{
    public class CommentDbContext : DbContext
    {
        string databaseName;

        public CommentDbContext()
            : base()
        {
        }

        public CommentDbContext(string databaseName)
            : base()
        {
            this.databaseName = databaseName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Config.GetConnectionString(databaseName ?? Config.DatabaseName)).AddInterceptors(new PersianInterceptor());
        }

        public ICollection<Holism.Social.Models.Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Holism.Social.Models.Comment>().ToTable("Comments");
            modelBuilder.Entity<Holism.Social.Models.Comment>().Ignore(i => i.RelatedItems);
			modelBuilder.Entity<Holism.Social.Models.Comment>().Property(i => i.PersianDate).HasComputedColumnSql("([dbo].[ToPersianDateTime]([Date]))");
			modelBuilder.Entity<Holism.Social.Models.Comment>().Property(i => i.Date).HasColumnType("datetime");
            base.OnModelCreating(modelBuilder);
        }
    }
}
