using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Scramjet.CrmPlugins {
    public static class PluginExecutionContextExtensions {
        public static Entity GetCurrentUser(this IPluginExecutionContext context, IOrganizationService org) {
            return (org.Retrieve("systemuser", context.UserId, new ColumnSet(true)));
        }
    }
}