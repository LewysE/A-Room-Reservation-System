using Calendar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Calendar.Data.Interfaces
{
    public interface IRoomRepo : IRepo<Room>
    {
        Task<List<Room>> GetByCampus(string campus);

        Task<List<Room>> GetByBuilding(string building);

        Task<List<Room>> GetByBuilding(string building, int ID);
    }
}
