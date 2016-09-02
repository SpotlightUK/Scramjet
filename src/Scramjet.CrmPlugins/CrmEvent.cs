using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace Scramjet.CrmPlugins {
    public class CrmEvent {
        public CrmEvent() { }
        public CrmEvent(IExecutionContext context, Entity editor) {
            MessageName = context.MessageName ?? "missing_message_name";
            CrmPluginDepth = context.Depth;
            CorrelationId = context.CorrelationId;
            EntityName = context.PrimaryEntityName;
            EntityId = context.PrimaryEntityId;
            ChangedBy = editor == null ? null : editor.ReadUsername();
            IsIntegrationUser = editor.IsIntegrationUser();
            ChangedAt = DateTimeOffset.Now;
        }

        /// <summary>The CRM plugin correlationID of the plugin execution that fired this notification</summary>
        public Guid CorrelationId { get; set; }

        /// <summary>The context.Depth of the plugin invocation that fired this notification.</summary>
        public Int32 CrmPluginDepth { get; set; }

        /// <summary>The EntityLogicalName of the CRM entity - "contact", "account", "salesorder", etc.</summary>
        public String EntityName { get; set; }

        /// <summary>The GUID identifying the entity that has been changed.</summary>
        public Guid EntityId { get; set; }

        /// <summary>The username of the CRM user who made this change.</summary>
        public String ChangedBy { get; set; }

        /// <summary>Was this changed made by an integration user or by a real person?</summary>
        public Boolean IsIntegrationUser { get; set; }

        /// <summary>The date/time when the change was submitted.</summary>
        public DateTimeOffset? ChangedAt { get; set; }

        /// <summary>The MessageName - "Create", "Update", "Delete", etc. - associated with the CRM plugin context that created this message.</summary>
        public String MessageName { get; set; }

        /// <summary>A collection of fields, including the entity ID field and any fields affected by the change.</summary>
        public Dictionary<String, Object> FieldChanges { get; set; } = new Dictionary<String, Object>();

        public override String ToString() {
            var sb = new StringBuilder();
            sb.AppendLine("entity: " + EntityName);
            sb.AppendLine("entityid: " + EntityId);
            sb.AppendLine("change: " + MessageName);
            sb.AppendLine();
            foreach (var f in FieldChanges.Where(f => f.Value != null)) {
                sb.AppendLine(f.Key + " = " + f.Value);
            }
            return sb.ToString();
        }
    }
}