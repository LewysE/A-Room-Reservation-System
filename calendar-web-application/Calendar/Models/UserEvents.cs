using Calendar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarWeb.Models
{
    public class UserEvents
    {
        [Key]
        public int Id { get; set; }

        public Room Room { get; set; }
        
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Owner { get; set; }

        public string Summary { get; set; }

        public string EventId { get; set; }
    }
}
