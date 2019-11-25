using Microsoft.EntityFrameworkCore;
using Ringer.Core.Models;

namespace Ringer.HubServer.Data
{
    public class RingerDbContext : DbContext
    {
        public RingerDbContext(DbContextOptions<RingerDbContext> options)
           : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        //public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            //modelBuilder.Entity<Ticket>().ToTable("Ticket");
        }
    }
}
