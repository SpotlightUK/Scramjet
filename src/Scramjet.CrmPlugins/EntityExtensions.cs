using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xrm.Sdk;

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

        public static readonly Dictionary<Type, Func<object, object>> formatters = new Dictionary<Type, Func<object, object>> {
            {
                typeof(EntityReference), value => new {
                    name = ((EntityReference)value).LogicalName,
                    id = Guid.Empty // ((EntityReference)value).Id
                }
            }, {
                typeof(Money), value => ((Money)value).Value
            },
            {
                typeof(OptionSetValue), value => ((OptionSetValue)value).Value
            },
            {
                typeof(DateTime), value => (DateTime)value
            },
            {
                typeof(DateTimeOffset), value => (DateTimeOffset)value
            }
        };



        public static Object Flatten(Object value) {
            if (formatters.ContainsKey(value.GetType())) return (formatters[value.GetType()](value));
            //if (value == null)
            //    return null;
            //if (value is EntityReference)
            //    ;
            //if (value is Money)
            //    return;
            //if (value is OptionSetValue)
            //    return ;
            //if (value is DateTime)
            //    return ((DateTime)value);
            //if (value is DateTimeOffset) {
            //    return ((DateTimeOffset)value);
            //}
            return value.ToString();
        }
    }
}