using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Scramjet.SampleWebApp.Controllers {
    public class HomeController : Controller {
        // GET: Home
        public ActionResult Index() {
            var jsonList = MvcApplication.Messages.Select(m => JsonConvert.SerializeObject(m, Formatting.Indented));
            return View(jsonList);
        }
    }
}