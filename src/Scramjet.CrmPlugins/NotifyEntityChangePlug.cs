using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
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

    public class Notification {
        public Notification(IExecutionContext context, Entity editor) {
            MessageName = context.MessageName ?? "missing_message_name";
            CrmPluginDepth = context.Depth;
            CorrelationId = context.CorrelationId;
            EntityName = context.PrimaryEntityName;
            EntityId = context.PrimaryEntityId;
            ChangedBy = (editor == null ? null : editor.ReadUsername());
            IsIntegrationUser = editor.IsIntegrationUser();
            ChangedAt = DateTimeOffset.Now;
        }

        /// <summary>The CRM plugin correlationID of the plugin execution that fired this notification</summary>
        public Guid CorrelationId {
            get;
            set;
        }

        /// <summary>The context.Depth of the plugin invocation that fired this notification.</summary>
        public int CrmPluginDepth {
            get;
            set;
        }

        /// <summary>The EntityLogicalName of the CRM entity - "contact", "account", "salesorder", etc.</summary>
        public string EntityName {
            get;
            set;
        }
        /// <summary>The GUID identifying the entity that has been changed.</summary>
        public Guid EntityId {
            get;
            set;
        }

        /// <summary>The username of the CRM user who made this change.</summary>
        public string ChangedBy {
            get;
            set;
        }

        /// <summary>Was this change caused by an integration user (normally via the API client), or by a real person using the web UI?</summary>
        public bool IsIntegrationUser {
            get;
            set;
        }
        /// <summary>The date/time when the change was submitted.</summary>
        public DateTimeOffset? ChangedAt { get; set; }

        /// <summary>The MessageName - "Create", "Update", "Delete", etc. - associated with the CRM plugin context that created this message.</summary>
        public string MessageName { get; set; }

        private Dictionary<string, string> fieldChanges = new Dictionary<string, string>();

        /// <summary>A collection of fields, including the entity ID field and any fields affected by the change.</summary>
        public Dictionary<string, string> FieldChanges {
            get {
                return fieldChanges;
            }
            set {
                fieldChanges = value;
            }
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine("entity: " + EntityName);
            sb.AppendLine("entityid: " + EntityId);
            sb.AppendLine("change: " + MessageName);
            sb.AppendLine();
            foreach (var f in fieldChanges.Where(f => f.Value != null)) {
                sb.AppendLine(f.Key + " = " + f.Value);
            }
            return (sb.ToString());
        }
    }

    public static class EntityExtensions {
        public static bool IsIntegrationUser(this Entity user) {
            return (user.Attributes.Contains("isintegrationuser") && Convert.ToBoolean(user.Attributes["isintegrationuser"]));
        }

        public static string ReadUsername(this Entity user) {
            return (user.Attributes.Contains("domainname") ? (string)user.Attributes["domainname"] : "(username_not_found)");
        }

        public static Dictionary<string, string> ToFieldChanges(this Entity entity) {
            return (entity.Attributes.ToDictionary(e => e.Key, e => Flatten(e.Value)));
        }

        public static string Flatten(object value) {
            if (value == null)
                return (null);
            if (value is EntityReference)
                return ((EntityReference)value).LogicalName + ":" + ((EntityReference)value).Id;
            if (value is Money)
                return ((Money)value).Value.ToString(CultureInfo.InvariantCulture);
            if (value is OptionSetValue)
                return ((OptionSetValue)value).Value.ToString(CultureInfo.InvariantCulture);
            if (value is DateTime)
                return ((DateTime)value).ToString("O");
            if (value is DateTimeOffset) {
                return ((DateTimeOffset)value).ToString("O");
            }
            return (value.ToString());
        }
    }
}