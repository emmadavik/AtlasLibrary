using Microsoft.EntityFrameworkCore;
using AtlasLibrary.UsersApi.Models;

namespace AtlasLibrary.UsersApi.Data
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options)
            : base(options)
        {
        }

        public DbSet<Anvandare> Anvandare { get; set; }
    }
}