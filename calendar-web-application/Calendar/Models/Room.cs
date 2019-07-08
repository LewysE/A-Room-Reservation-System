using CalendarWeb.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Calendar.Models
{
    public class Room
    {

        [Key]
        public int Id { get; set; }

        public int RoomNumber { get; set; }

        public virtual Building Building { get; set; }

        [Display(Name = "Room Capacity")]
        public int Capacity { get; set; }

        public string CalendarId { get; set; }
    }
}
