using Calendar.Data;
using Calendar.Data.Repos;
using CalendarWeb.Data.Interfaces;
using CalendarWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarWeb.Data.Repos
{
    public class BuildingRepo : Repo<Building>, IBuildingRepo

    {
        public BuildingRepo(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<List<Building>> GetByCampusAsync(string campus)
        {
            return await _context.Buildings.Where(m => m.Campus.Name == campus).ToListAsync();
        }

        public async override Task<Building> GetById(int? id)
        {

            return await _context.Buildings.Include(a => a.Campus).FirstOrDefaultAsync(a => a.ID == id);
        }
    }
}
