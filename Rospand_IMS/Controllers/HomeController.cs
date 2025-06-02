using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rospand_IMS.Data;
using Rospand_IMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace Rospand_IMS.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            
            return View();
        }

/*
        [HttpGet]
        public JsonResult GetStatesByCountry(int countryId)
        {
            var states = _context.States
                .Where(s => s.CountryId == countryId)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();
            return Json(states);
        }

        [HttpGet]
        public JsonResult GetCitiesByState(int stateId)
        {
            var cities = _context.Cities
                .Where(c => c.StateId == stateId)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToList();
            return Json(cities);
        }*/
        public IActionResult Privacy()
        {
            return View();
        }
    }

    public class DashboardViewModel
    {
        public string Period { get; set; }
        public int PurchaseOrderCount { get; set; }
        public int SalesOrderCount { get; set; }
        public int OutwardEntryCount { get; set; }
        public int TotalStock { get; set; }
        public decimal PurchaseTotal { get; set; }
        public decimal SalesTotal { get; set; }
     
        public List<string> TrendLabels { get; set; }
        public List<decimal> PurchaseTrends { get; set; }
        public List<decimal> SalesTrends { get; set; }
    }
}