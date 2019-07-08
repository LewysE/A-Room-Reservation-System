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
    public class EventsRepo : Repo<UserEvents>, IEventRepo
    {
        public EventsRepo(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<UserEvents>> GetByUserAsync(string user)
        {
            return  await _context.Events.Where(M => M.Owner == user).Include(X => X.Room).OrderBy(s => s.StartTime).ToListAsync(); 
        }


        public async override Task<UserEvents> GetById(int? id)
        {

            return await _context.Events.Include(a => a.Room).FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}
