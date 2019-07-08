using Calendar.Data.Interfaces;
using Calendar.Models;
using Calendar.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalendarWeb.Models;

namespace Calendar.Data
{
    public class DbSeeder
    {

        public static void SeedDb(ApplicationDbContext context)
        {
            context.Database.EnsureCreatedAsync().Wait();

            if (!context.Campus.Any())
            {
                Campus singleton = new Campus
                {
                    Name = LocationConstants.SingletonCampus,
                };

                context.Campus.AddAsync(singleton).Wait();
                context.SaveChangesAsync().Wait();
                Campus bay = new Campus
                {
                    Name = LocationConstants.BayCampus,

                };

                context.Campus.AddAsync(bay).Wait();
                context.SaveChangesAsync().Wait();


                SeedBuilding(context, singleton, LocationConstants.GroveBuilding);
                SeedBuilding(context, singleton, LocationConstants.FultonHouse);
                SeedBuilding(context, singleton, LocationConstants.WallaceBuilding);
                SeedBuilding(context, singleton, LocationConstants.ComputationalFoundry);
                SeedBuilding(context, bay, LocationConstants.BayLibary);
                SeedBuilding(context, bay, LocationConstants.GreatHall);
                SeedBuilding(context, bay, LocationConstants.SchoolOfManagment);
                SeedBuilding(context, bay, LocationConstants.TheCollege);

            }

        }

        private static void SeedBuilding(ApplicationDbContext context, Campus campus, string building)
        {
            Building newbuilding = new Building
            {
                Campus = campus,
                Name = building
            };

            context.Buildings.AddAsync(newbuilding).Wait();
            context.SaveChangesAsync().Wait();
        }

    }
}
