using Microsoft.EntityFrameworkCore;
using LaLigaTrackerBackend.Models;

namespace LaLigaTrackerBackend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Match> Matches { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}