using InkCards.Models.Testing;
using Microsoft.EntityFrameworkCore;

namespace InkCards.Services.Storage.Sqlite
{
    class MainDatabaseContext : DbContext
    {
        public DbSet<CardImpression> CardImpressions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite("Filename=Main.db");
    }
}
