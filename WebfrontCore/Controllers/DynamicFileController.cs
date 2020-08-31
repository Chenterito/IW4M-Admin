﻿using Microsoft.AspNetCore.Mvc;
using SharedLibraryCore;
using SharedLibraryCore.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebfrontCore.Controllers
{
    [Route("dynamic")]
    public class DynamicFileController : BaseController
    {
        private static readonly IDictionary<string, string> _fileCache = new Dictionary<string, string>();

        public DynamicFileController(IManager manager) : base(manager)
        {

        }

        [Route("css/{fileName}")]
        public async Task<IActionResult> Css(string fileName)
        {
            if (fileName.EndsWith(".css"))
            {
                if (Utilities.IsDevelopment)
                {
                    var path = Path.Join(Utilities.OperatingDirectory, "..", "..", "..", "..", "WebfrontCore", "wwwroot", "css", fileName);
                    string cssData = await System.IO.File.ReadAllTextAsync(path);
                    cssData = await Manager.MiddlewareActionHandler.Execute(cssData, "custom_css_accent");
                    return Content(cssData, "text/css");
                }

                if (!_fileCache.ContainsKey(fileName))
                {

                    string path = $"wwwroot{Path.DirectorySeparatorChar}css{Path.DirectorySeparatorChar}{fileName}";
                    string data = await System.IO.File.ReadAllTextAsync(path);
                    data = await Manager.MiddlewareActionHandler.Execute(data, "custom_css_accent");
                    _fileCache.Add(fileName, data);
                }

                return Content(_fileCache[fileName], "text/css");
            }

            return StatusCode(400);
        }
    }
}
