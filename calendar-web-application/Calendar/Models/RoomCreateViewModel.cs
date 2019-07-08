using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarWeb.Models
{
    public class RoomCreateViewModel
    {
        public int RoomNumber { get; set; }

        public Building Building { get; set; }

        public int BuildingID { get; set; }

        public int Capacity { get; set; }

        public string CalendarId { get; set; }

        public List<Building> buildings { get; set; }


    }

    
}
