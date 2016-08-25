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
                ? (String) user.Attributes["domainname"]
                : "(username_not_found)";
        }

        public static Dictionary<String, String> ToFieldChanges(this Entity entity) {
            return entity.Attributes.ToDictionary(e => e.Key, e => Flatten(e.Value));
        }

        public static String Flatten(Object value) {
            if (value == null)
                return null;
            if (value is EntityReference)
                return ((EntityReference) value).LogicalName + ":" + ((EntityReference) value).Id;
            if (value is Money)
                return ((Money) value).Value.ToString(CultureInfo.InvariantCulture);
            if (value is OptionSetValue)
                return ((OptionSetValue) value).Value.ToString(CultureInfo.InvariantCulture);
            if (value is DateTime)
                return ((DateTime) value).ToString("O");
            if (value is DateTimeOffset) {
                return ((DateTimeOffset) value).ToString("O");
            }
            return value.ToString();
        }
    }
}