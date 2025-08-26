using ELRakhawy.EL.Interfaces;
using ELRakhawy.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ELRakhawy.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Display Public tables 
        public IActionResult PublicTables()
        {
            return View();
        }

        // Number Formatting Demo

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
