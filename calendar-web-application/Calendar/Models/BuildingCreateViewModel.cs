using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarWeb.Models
{
    public class BuildingCreateViewModel
    {
        public string Name { get; set; }

        public Campus Campus { get; set; }

        public int CampusID { get; set; }

        public List<Campus> Campuses { get; set; }

    }
}
