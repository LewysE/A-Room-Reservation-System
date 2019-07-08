using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarWeb.Models
{
    public class Building
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual Campus Campus { get; set; }
    }
}
