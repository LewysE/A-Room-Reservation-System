using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Calendar.Data;
using Calendar.Models;

using System.IO;

using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CalendarWeb.Models;
using CalendarWeb.Data.Interfaces;
using Newtonsoft.Json;
using Calendar.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Calendar.Controllers
{
    public class RoomsController : Controller 
    {
        private readonly ApplicationDbContext _context;
        private readonly IBuildingRepo _buildingRepo;
        private readonly IRoomRepo _roomRepo;
        private readonly IEventRepo _eventRepo;
        private CalendarService service;

        public RoomsController(ApplicationDbContext context, IBuildingRepo buildingRepo, IRoomRepo roomRepo, IEventRepo eventRepo)
        {
            _context = context;
            _buildingRepo = buildingRepo;
            _roomRepo = roomRepo;
            _eventRepo = eventRepo;

            service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = ApplicationCredentials.GetCredential(),
                ApplicationName = ApplicationCredentials.ApplicationName,
            });
        }

        // GET: Rooms
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var campus = await _context.Campus
                .FirstOrDefaultAsync(m => m.ID == id);
            if (campus == null)
            {
                return NotFound();
            }


            RoomViewModel viewModel = new RoomViewModel
            {
                Buildings = await _buildingRepo.GetByCampusAsync(campus.Name),
                Rooms = await _roomRepo.GetByCampus(campus.Name)
            };

            return View(viewModel);
        }

        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            RoomDetailsViewModel viewModel = new RoomDetailsViewModel();

            var room = await _roomRepo.GetById(id);
            if (room == null)
            {
                return NotFound();
            }

            viewModel.Room = room;
            var now = DateTime.Now;


            // Return a list of todays events
            EventsResource.ListRequest request = service.Events.List(viewModel.Room.CalendarId);
            request.TimeMin = now.ToUniversalTime();
            request.TimeMax = DateTime.Today.AddDays(7).ToUniversalTime();
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            viewModel.Events = request.Execute();

            
            
            if(viewModel.Events.Items.Count > 0)
            {
                if (viewModel.Events.Items[0].Start.DateTime < now)
                {
                    viewModel.Nearest = await NearestEmpty(viewModel);
                }
            }

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, user")]
        public async Task<IActionResult> Details([Bind("RoomId,Summary,start,end,date")] RoomDetailsViewModel roomViewModel)
        {
            if (ModelState.IsValid)
            {
                var temp = DateTime.Now;
                var room = await _roomRepo.GetById(roomViewModel.RoomId);
                if (room == null)
                {
                    return NotFound();
                }
                roomViewModel.Room = room;


                System.Diagnostics.Debug.WriteLine("\n\n "+ roomViewModel.date+ "\n\n");

                if (roomViewModel.date.Day > temp.Day)
                {
                    
                    int diff = roomViewModel.date.Day - temp.Day;
                    var tempstart = roomViewModel.start.AddDays(diff);
                    var tempend = roomViewModel.end.AddDays(diff);
                    roomViewModel.start =  tempstart;
                    roomViewModel.end = tempend;

                    System.Diagnostics.Debug.WriteLine("\n\n" + roomViewModel.start.ToUniversalTime() + " \n\n");
                }

                Event newEvent = new Event
                {
                    Summary = roomViewModel.Summary,
                    Start = new EventDateTime()
                    {
                        DateTime = roomViewModel.start.ToUniversalTime(),
                        TimeZone = "Europe/London"
                    },
                    End = new EventDateTime()
                    {
                        DateTime = roomViewModel.end.ToUniversalTime(),
                        TimeZone = "Europe/London"
                    },
                };

                Event.ExtendedPropertiesData exp = new Event.ExtendedPropertiesData();
                exp.Shared = new Dictionary<string, string>();
                exp.Shared.Add("owner", User.Identity.Name);
                newEvent.ExtendedProperties = exp;
                Event addEvent = service.Events.Insert(newEvent, roomViewModel.Room.CalendarId).Execute();

                UserEvents events = new UserEvents
                {
                    Room = roomViewModel.Room,
                    StartTime = roomViewModel.start,
                    EndTime = roomViewModel.end,
                    Summary = roomViewModel.Summary,
                    Owner = User.Identity.Name,
                    EventId = addEvent.Id
                };

                await _eventRepo.Add(events);

                return RedirectToAction("Details", roomViewModel.RoomId);
            }
            return View(roomViewModel);
        }

        // GET: Rooms/Add
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create()
        {
            RoomCreateViewModel viewModel = new RoomCreateViewModel();
            viewModel.buildings =  await _buildingRepo.GetAll();

            return View(viewModel);
        }

        // POST: Rooms/Add
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("Id,RoomNumber,BuildingID,Capacity")] RoomCreateViewModel roomViewModel)
        {
            if (ModelState.IsValid)
            {
                // get the building from the buidling id
                var temp = await _buildingRepo.GetById(roomViewModel.BuildingID);
               
                Dictionary<string, string> location = new Dictionary<string, string>
                {
                    {"campus", temp.Campus.Name },
                    {"building", temp.Name }
                };

                // Convert the calendars location into a JSON string
                string str = JsonConvert.SerializeObject(location, Formatting.Indented);

                // create the calendar with the Google API
                Google.Apis.Calendar.v3.Data.Calendar calendar = new Google.Apis.Calendar.v3.Data.Calendar
                {
                    Summary = roomViewModel.RoomNumber.ToString(),
                    TimeZone = "Europe/London",
                    Description = str
                };

                
                Google.Apis.Calendar.v3.Data.Calendar addCalendar = service.Calendars.Insert(calendar).Execute();
                
                // Add the room to the database
                Room room = new Room
                {
                    RoomNumber = roomViewModel.RoomNumber,
                    Building = temp,
                    CalendarId = addCalendar.Id,
                    Capacity = roomViewModel.Capacity,

                };

                await _roomRepo.Add(room);
                return RedirectToAction("Index", "Home");
            }
            return View(roomViewModel);
        }

        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomNumber,RoomProperties,CalendarId")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Rooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            service.Calendars.Delete(room.CalendarId).ExecuteAsync().Wait();



            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }


        private async Task<Room> NearestEmpty(RoomDetailsViewModel viewModel)
        {
            // get a list of rooms in the building expect the specified room
            List<Room> sameBuilding = await _roomRepo.GetByBuilding(
                                                    viewModel.Room.Building.Name, viewModel.RoomId);
            List<Room> emptyRoom = new List<Room>();

            var temp = DateTime.Now;
            
            DateTime time = temp.AddMinutes(-temp.Minute).AddSeconds(-temp.Second)
                                .AddMilliseconds(-temp.Millisecond).ToUniversalTime();

            foreach (Room room in sameBuilding)
            {
                EventsResource.ListRequest request = service.Events.List(room.CalendarId);
                request.TimeMin = temp;
                request.TimeMax = time.AddHours(1);
                request.ShowDeleted = false;

                Events events = request.Execute();
                // If there are no events currently
                if(events.Items.Count == 0)
                {
                    emptyRoom.Add(room);
                }
            }

            Room nearest = null;

            if (emptyRoom.Count > 0)
            {
                int i = 1;
                while(nearest == null) {
                    foreach (Room room in emptyRoom)
                    {
                        if (viewModel.Room.RoomNumber + 1 == room.RoomNumber)
                        {
                            nearest = room;
                            break;
                        }
                        else if (viewModel.Room.RoomNumber - i == room.RoomNumber)
                        {
                            nearest = room;
                            break;
                        }
                    }
                    i++;
                }
            }
            return nearest;
        }
    }
}
