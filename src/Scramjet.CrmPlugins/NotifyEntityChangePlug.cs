using System;
using System.Net;
using System.Reflection;
using System.Text;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;

namespace Scramjet.CrmPlugins {
    public class NotifyEntityChangePlugin : PluginBase {
        // Delete actions triggered by workflows and bulk delete operations 
        // happen with depth = 3, so we need to increase our limit accordingly.
        // 3 didn't do any good, so we've cranked it up to 16 in the hope it'll shed
        // some light on why contract extension events aren't firing on production CRM.
        private const int MAX_DEPTH = 16;

        protected override void Execute(LocalPluginContext localPluginContext) {
            if (localPluginContext == null) throw new ArgumentNullException(nameof(localPluginContext));
            var context = localPluginContext.PluginExecutionContext;
            if (context == null || context.Depth > MAX_DEPTH) return;

            var org = localPluginContext.OrganizationService;
            var editor = context.GetCurrentUser(org);
            var target = (context.InputParameters.Contains("Target") ? context.InputParameters["Target"] : new EntityReference("missing_target", Guid.Empty));

            var change = new Notification(context, editor);
            if (target is Entity) change.FieldChanges = ((Entity)target).ToFieldChanges();

            PostChangesToWebhook(change);
        }

        public void PostChangesToWebhook(object change) {
            new WebClient().UploadString("http://requestb.in/1fwurhm1", JsonConvert.SerializeObject(change));
        }
    }
}