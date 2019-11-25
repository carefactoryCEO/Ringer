using System;
using System.Linq;
using Ringer.Core.Models;

namespace Ringer.PartnerWeb.Data
{
    public static class DbInitializer
    {
        public static void Initialize(PartnerContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;
            }

            var Users = new User[]
            {
                new User{Name="신모범", BirthDate=DateTime.Parse("1976-07-21"), Gender=GenderType.Male, PhoneNumber="01062217046"},
                new User{Name="김순용", BirthDate=DateTime.Parse("1980-08-20"), Gender=GenderType.Male, PhoneNumber="01012345678"},
                new User{Name="김은미", BirthDate=DateTime.Parse("1981-06-25"), Gender=GenderType.Female, PhoneNumber="01072313805"}
            };

            foreach (User u in Users)
            {
                context.Users.Add(u);
            }

            context.SaveChanges();
        }
    }
}
