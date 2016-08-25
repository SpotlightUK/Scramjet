using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;

namespace Scramjet.CrmPlugins {
    public class NotifyEntityChangePlugin : PluginBase {
        protected override void Execute(LocalPluginContext context) {
            var web = new WebClient();
            var pc = context.PluginExecutionContext;
            var sb = new StringBuilder();
            sb.AppendLine("CorrelactionId: " + pc.CorrelationId);
            sb.AppendLine("EntityName " + pc.PrimaryEntityName);
            sb.AppendLine("EntityId " + pc.PrimaryEntityId);
            foreach (var propertyInfo in pc.GetType().GetProperties(BindingFlags.FlattenHierarchy)) {
                sb.AppendLine(propertyInfo.Name + " = " + propertyInfo.GetValue(pc, null));
            }
            web.UploadString("http://requestb.in/1fwurhm1", sb.ToString());
        }
    }

    //public class ScramjetConfiguration {
    //    private OrganizationServiceContext context;
    //    public ScramjetConfiguration(IOrganizationService service) {
    //        this.context = new OrganizationServiceContext(service);
    //    }

    //    const string KEY = "scramjet_configuration_key";
    //    const string VALUE = "scramjet_configuration_value";

    //    public string Setting(string key) {
    //        var setting = context.CreateQuery("scramjet_configuration")
    //            .Where(c => c.Attributes.ContainsKey(KEY) && c.Attributes[KEY] == key)
    //            .Select(c => c.Attributes[VALUE]).FirstOrDefault();
    //        if (setting == null) throw new KeyNotFoundException(key);
    //        return ((string)setting);
    //    }
    //}
}