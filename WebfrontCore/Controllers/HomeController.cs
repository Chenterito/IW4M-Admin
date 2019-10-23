﻿using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SharedLibraryCore.Dtos;
using System.Linq;
using System.Threading.Tasks;

namespace WebfrontCore.Controllers
{
    public class HomeController : BaseController
    {
        public async Task<IActionResult> Index()
        {
            ViewBag.Description = "IW4MAdmin is a complete server administration tool for IW4x.";
            ViewBag.Title = Localization["WEBFRONT_HOME_TITLE"];
            ViewBag.Keywords = "IW4MAdmin, server, administration, IW4x, MW2, Modern Warfare 2";

            var model = new IW4MAdminInfo()
            {
                TotalAvailableClientSlots = Manager.GetServers().Sum(_server => _server.MaxClients),
                TotalOccupiedClientSlots = Manager.GetActiveClients().Count,
                TotalClientCount = await Manager.GetClientService().GetTotalClientsAsync(),
                RecentClientCount = await Manager.GetClientService().GetRecentClientCount()
            };

            return View(model);
        }

        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            Manager.GetLogger(0).WriteError($"[Webfront] {exceptionFeature.Error.Message}");
            Manager.GetLogger(0).WriteDebug(exceptionFeature.Path);
            Manager.GetLogger(0).WriteDebug(exceptionFeature.Error.StackTrace);

            ViewBag.Description = Localization["WEBFRONT_ERROR_DESC"];
            ViewBag.Title = Localization["WEBFRONT_ERROR_TITLE"];
            return View(exceptionFeature.Error);
        }

        public IActionResult ResponseStatusCode(int? statusCode = null)
        {
            return View(statusCode);
        }
    }
}
