using Calendar.Data.Interfaces;
using CalendarWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarWeb.Data.Interfaces
{
    public interface IEventRepo : IRepo<UserEvents>
    {
        Task<List<UserEvents>> GetByUserAsync(string user);

    }
}
