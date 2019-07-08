using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Calendar.Models;
using Calendar.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using CalendarWeb.Models;
using Google.Apis.Calendar.v3;
using CalendarWeb.Data.Interfaces;
using Google.Apis.Services;
using Calendar.Data;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Controllers
{

    [Authorize]
    public class HomeController : Controller
    {


        public HomeController()
        {
        }
       


        [AllowAnonymous]
        public IActionResult Index()
        {
            
            return View();
        }


        [Authorize(Roles = "admin")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [Authorize(Roles = "user")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
