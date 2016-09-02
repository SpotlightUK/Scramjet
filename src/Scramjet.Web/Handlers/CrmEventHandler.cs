using System.IO;
using System.Web;
using Newtonsoft.Json;
using Scramjet.CrmPlugins;

namespace Scramjet.Web.Handlers {
    public class CrmEventHttpHandler : IHttpHandler {

        public static event CrmEventHandler CrmEventReceived;

        public void ProcessRequest(HttpContext context) {
            var handler = CrmEventReceived;
            if (handler == null) return;
            var jsonData = new StreamReader(context.Request.InputStream).ReadToEnd();
            var crmEvent = JsonConvert.DeserializeObject<CrmEvent>(jsonData);
            var args = new CrmEventArgs(crmEvent);
            handler(args);
        }

        public bool IsReusable { get; }
    }
}
