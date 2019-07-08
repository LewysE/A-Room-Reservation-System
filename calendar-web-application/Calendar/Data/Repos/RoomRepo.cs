using Calendar.Data.Interfaces;
using Calendar.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Calendar.Data.Repos
{
    public class RoomRepo : Repo<Room>, IRoomRepo
    {
        public RoomRepo(ApplicationDbContext context) : base(context)
        {
        }
        public override async Task<Room> GetById(int? id)
        {
            return await _context.Rooms.Include(a => a.Building).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Room>> GetByCampus(string campus)
        {
            return await _context.Rooms.Where(M => M.Building.Campus.Name == campus).ToListAsync();
        }

        public async Task<List<Room>> GetByBuilding(string buidling)
        {
            return await _context.Rooms.Where(M => M.Building.Name == buidling).Include(a => a.Building).ToListAsync();
        }

        // gets all
        public async Task<List<Room>> GetByBuilding(string buidling, int ID)
        {
            return await _context.Rooms.Where(M => M.Building.Name == buidling).Where(X => X.Id != ID).Include(a => a.Building).ToListAsync();
        }
    }
}
