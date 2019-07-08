using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Calendar.Data;
using CalendarWeb.Models;
using CalendarWeb.Data.Interfaces;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace CalendarWeb.Controllers
{
    public class MyEventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEventRepo _eventRepo;
        private CalendarService service;

        public MyEventsController(ApplicationDbContext context, IEventRepo eventRepo)
        {
            _context = context;
            _eventRepo = eventRepo;

            service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = ApplicationCredentials.GetCredential(),
                ApplicationName = ApplicationCredentials.ApplicationName,
            });
        }

        // GET: MyEvents
        public async Task<IActionResult> Index()
        {

            List<UserEvents> temp = await _eventRepo.GetByUserAsync(User.Identity.Name);

            MyEventsModelView viewModel = new MyEventsModelView();

            viewModel.CurrentEvents = new List<UserEvents>();
            viewModel.PreviousEvents = new List<UserEvents>();

            foreach (UserEvents item in temp)
            {
                if(DateTime.Now < item.EndTime)
                {
                    viewModel.CurrentEvents.Add(item);
                } else
                {
                    viewModel.PreviousEvents.Add(item);
                }
            }



            return View(viewModel);
        }

        // GET: MyEvents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userEvents = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userEvents == null)
            {
                return NotFound();
            }

            return View(userEvents);
        }

        // GET: MyEvents/Add
        public IActionResult Create()
        {
            return View();
        }

        // POST: MyEvents/Add
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StartTime,EndTime,Owner,EventId")] UserEvents userEvents)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userEvents);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userEvents);
        }

        // GET: MyEvents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userEvents = await _context.Events.FindAsync(id);
            if (userEvents == null)
            {
                return NotFound();
            }
            return View(userEvents);
        }

        // POST: MyEvents/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartTime,EndTime,Owner,EventId")] UserEvents userEvents)
        {
            if (id != userEvents.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userEvents);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserEventsExists(userEvents.Id))
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
            return View(userEvents);
        }

        // GET: MyEvents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userEvents = await _context.Events.Include(a => a.Room).Include(a=> a.Room.Building)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userEvents == null)
            {
                return NotFound();
            }

            return View(userEvents);
        }

        // POST: MyEvents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            // get the event
            var userEvents = await _context.Events.Include(x => x.Room).FirstOrDefaultAsync(m => m.Id == id);
            System.Diagnostics.Debug.WriteLine("\n\n" + id + "\n\n");
            // delete the event on the Google Calendar
            service.Events.Delete(userEvents.Room.CalendarId, userEvents.EventId).ExecuteAsync().Wait();
            // delete event from the database
            _context.Events.Remove(userEvents);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost, ActionName("Clear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearConfirmed()
        {


            List<UserEvents> temp = await _eventRepo.GetByUserAsync(User.Identity.Name);


            foreach (UserEvents item in temp)
            {
                if (DateTime.Now > item.EndTime)
                {
                    await _eventRepo.Delete(item);
                }

            }


            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool UserEventsExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
