using System;
using Scramjet.CrmPlugins;

namespace Scramjet.Web {
    /// <summary>Event raised when a notification is received from the Scramjet CRM plugin.</summary>
    public class CrmEventArgs : EventArgs {
        private readonly CrmEvent crmEvent;
        public CrmEventArgs(CrmEvent crmEvent) {
            this.crmEvent = crmEvent;
        }
        public CrmEvent CrmEvent { get { return (crmEvent); } }
    }
}