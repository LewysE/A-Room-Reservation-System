using Calendar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarWeb.Models
{
    public class RoomViewModel
    {
        public List<Building> Buildings { get; set; }
        public List<Room> Rooms { get; set; }
    }
}
