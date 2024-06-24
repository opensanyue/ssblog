using aspnetMCVBSUser1.Models;
using aspnetMCVBSUser1.Models.ViewModels;
using aspnetMCVBSUser1.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace aspnetMCVBSUser1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOptionsSnapshot<ConfigModel> model;

        public HomeController(IOptionsSnapshot<ConfigModel> config, ILogger<HomeController> logger)
        {
            _logger = logger;
            this.model = config;
        }

        public IActionResult Index()
        {
            ViewData["AppName"] = model.Value.AppName;
            return View();
        }
        public IActionResult Blog()
        {
            ViewData["AppName"] = model.Value.AppName;
            return View();
        }
        [Authorize]
        public IActionResult Privacy()
        {
            ViewData["AppName"] = model.Value.AppName;
            return View();
        }

       
        public IActionResult About()
        {
            ViewData["Title"] = "¹ØÓÚ";
            ViewData["AppName"] = PubServer.Setting.AppName;
            ViewData["WelcomeSpeech"] = PubServer.WelcomeSpeech;
            ViewData["Grade"] = PubServer.lic != null ? PubServer.lic.Grade : 0;
            ViewData["Content"] = PubServer.lic != null ? PubServer.lic.Content : "";
            return View();
        }

        public IActionResult Help()
        {
            ViewData["Title"] = "°ïÖú";
            ViewData["AppName"] = PubServer.Setting.AppName;
            ViewData["WelcomeSpeech"] = PubServer.WelcomeSpeech;
            ViewData["Grade"] = PubServer.lic != null ? PubServer.lic.Grade : 0;
            ViewData["Content"] = PubServer.lic != null ? PubServer.lic.Content : "";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            ViewData["AppName"] = model.Value.AppName;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
