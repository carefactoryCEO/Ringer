using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ringer.Core.Data;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .Property(p => p.Gender)
                .HasConversion(new EnumToStringConverter<GenderType>());

            modelBuilder
                .Entity<User>()
                .Property(p => p.UserType)
                .HasDefaultValue(UserType.Consumer)
                .HasConversion(new EnumToStringConverter<UserType>());

            modelBuilder.Entity<User>().ToTable("User");
        }
    }
}