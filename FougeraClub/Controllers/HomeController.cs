
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FougeraClub.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
  
       

        public async Task<IActionResult> Index()
        {

            return View();
        }

        public async Task<IActionResult> AddEdit()
        {
            
            return View();
        }

       

        
    }
}