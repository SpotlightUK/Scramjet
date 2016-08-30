using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Scramjet.Web.Handlers;

namespace Scramjet.SampleWebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            CrmEventHttpHandler.CrmEventReceived += CrmEventHttpHandler_CrmEventReceived;
        }

        public static List<string> Messages = new List<string>();

        private void CrmEventHttpHandler_CrmEventReceived(Web.CrmEventArgs args) {
            Messages.Add(args.CrmEvent.ToString());
        }
    }
}
