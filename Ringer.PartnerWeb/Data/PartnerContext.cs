using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Ringer.Core.Models;

namespace Ringer.PartnerWeb.Data
{
    public class PartnerContext : DbContext
    {
        public PartnerContext(DbContextOptions<PartnerContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Ticket>().ToTable("Ticket");
        }
    }
}
