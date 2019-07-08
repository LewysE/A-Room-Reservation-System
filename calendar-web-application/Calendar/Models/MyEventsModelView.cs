using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarWeb.Models
{
    public class MyEventsModelView
    {
        public List<UserEvents> PreviousEvents { get; set; }
        public List<UserEvents> CurrentEvents { get; set; }
        public UserEvents test { get; set; }
        public int Id { get; set; }
    }
}
