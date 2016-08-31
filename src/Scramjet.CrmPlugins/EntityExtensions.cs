using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xrm.Sdk;
using crm = Microsoft.Xrm.Sdk;

namespace Scramjet.CrmPlugins {
    public static class EntityExtensions {
        public static Boolean IsIntegrationUser(this Entity user) {
            return user.Attributes.Contains("isintegrationuser") &&
                   Convert.ToBoolean(user.Attributes["isintegrationuser"]);
        }

        public static String ReadUsername(this Entity user) {
            return user.Attributes.Contains("domainname")
                ? (String)user.Attributes["domainname"]
                : "(username_not_found)";
        }

        public static Dictionary<String, Object> ToFieldChanges(this Entity entity) {
            return entity.Attributes.ToDictionary(e => e.Key, e => e.Value);
        }

        public static object FlattenEntityReference(crm.EntityReference value) {
            return (new {
                name = ((crm.EntityReference)value).LogicalName,
                guid = ((crm.EntityReference)value).Id
            });
        }

        private static readonly Dictionary<Type, Func<object, object>> Formatters = new Dictionary<Type, Func<object, object>> {
            { typeof(Money), value => ((Money)value).Value },
            { typeof(OptionSetValue), value => ((OptionSetValue)value).Value },
            { typeof(DateTime), value => (DateTime)value },
            { typeof(DateTimeOffset), value => (DateTimeOffset)value },
            {typeof(EntityReference), value => new ScramjetEntityReference(
                ((EntityReference)value).LogicalName,
                ((EntityReference)value).Id
                ) }
        };

        public static object Flatten(Object value) {
            return Formatters.ContainsKey(value.GetType()) ? (Formatters[value.GetType()](value)) : value;
        }
    }

    public struct ScramjetEntityReference {
        public ScramjetEntityReference(string name, Guid guid) {
            this.Name = name;
            this.Guid = guid;
        }
        public string Name { get; set; }
        public Guid Guid { get; set; }

    }
}