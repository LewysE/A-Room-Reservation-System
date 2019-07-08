using Calendar.Models;
using Google.Apis.Calendar.v3.Data;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarWeb.Models
{
    public class RoomDetailsViewModel
    {
        public Room Room { get; set; }
        public Events Events { get; set; }
        public Room Nearest { get; set; }

        public string Summary { get; set; }

        public int RoomId { get; set; }


        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public DateTime date { get; set; }


        public RoomDetailsViewModel()
        {
            var temp = DateTime.Now;
            Summary = "";
            date = DateTime.Today;
            start = DateTime.Now.AddMilliseconds(-temp.Millisecond).AddSeconds(-temp.Second);
            end = DateTime.Now.AddMilliseconds(-temp.Millisecond).AddSeconds(-temp.Second).AddMinutes(10);

        }
    }


    public class DateMinAttribute : RangeAttribute
    {
        public DateMinAttribute()
          : base(typeof(DateTime), DateTime.Now.TimeOfDay.ToString(), DateTime.Now.AddYears(2).ToShortDateString()) { }
    }

}
