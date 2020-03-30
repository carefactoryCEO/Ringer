using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ringer.Core.Data;
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
        public DbSet<Message> Messages { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Consulate> Consulates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<User>()
                .Property(p => p.Gender)
                .HasConversion(new EnumToStringConverter<GenderType>());

            modelBuilder.Entity<User>()
                .Property(p => p.UserType)
                .HasDefaultValue(UserType.Consumer)
                .HasConversion(new EnumToStringConverter<UserType>());

            modelBuilder.Entity<User>()
                .ToTable("User")
                .HasData
                (
                    new User { Id = 1, Name = "Admin", BirthDate = DateTime.Parse("1976-07-21"), Gender = GenderType.Male, CreatedAt = DateTime.UtcNow, UserType = UserType.Admin },
                    new User { Id = 2, Name = "신모범", BirthDate = DateTime.Parse("1976-07-21"), Gender = GenderType.Male, CreatedAt = DateTime.UtcNow },
                    new User { Id = 3, Name = "김은미", BirthDate = DateTime.Parse("1981-06-25"), Gender = GenderType.Female, CreatedAt = DateTime.UtcNow },
                    new User { Id = 4, Name = "김순용", BirthDate = DateTime.Parse("1980-07-04"), Gender = GenderType.Male, CreatedAt = DateTime.UtcNow },
                    new User { Id = 5, Name = "함주희", BirthDate = DateTime.Parse("1981-12-25"), Gender = GenderType.Female, CreatedAt = DateTime.UtcNow }
                );

            // Message
            modelBuilder.Entity<Message>().ToTable("Message");

            // Device
            modelBuilder.Entity<Device>()
                .Property(d => d.DeviceType)
                .HasConversion(new EnumToStringConverter<DeviceType>());

            modelBuilder.Entity<Device>().ToTable("Device");

            // Room
            modelBuilder.Entity<Room>()
                .ToTable("Room");

            // Consulate
            modelBuilder.Entity<Consulate>()
                .HasIndex(c => new { c.CountryCode, c.CountryCodeiOS, c.CountryCodeAndroid });

            modelBuilder.Entity<Consulate>()
                .ToTable("Consulate");

            modelBuilder.Entity<Consulate>()
                .Ignore(c => c.Distance);
        }
    }
}
