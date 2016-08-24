using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;

namespace Scramjet.CrmPlugins {
    /// <summary>Base class for all plug-in classes.</summary>    
    public abstract class PluginBase : IPlugin {
        /// <summary>
        /// Plug-in context object. 
        /// </summary>
        protected class LocalPluginContext {
            internal IServiceProvider ServiceProvider { get; private set; }

            /// <summary>The Microsoft Dynamics CRM organization service.</summary>
            internal IOrganizationService OrganizationService { get; private set; }

            /// <summary>IPluginExecutionContext contains information that describes the run-time environment in which the plug-in executes, information related to the execution pipeline, and entity business information.</summary>
            internal IPluginExecutionContext PluginExecutionContext { get; private set; }

            /// <summary>
            /// Synchronous registered plug-ins can post the execution context to the Microsoft Azure Service Bus. <br/> 
            /// It is through this notification service that synchronous plug-ins can send brokered messages to the Microsoft Azure Service Bus.
            /// </summary>
            internal IServiceEndpointNotificationService NotificationService { get; private set; }

            /// <summary>Provides logging run-time trace information for plug-ins.</summary>
            internal ITracingService TracingService { get; private set; }

            internal LocalPluginContext(IServiceProvider serviceProvider) {
                if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));                

                // Obtain the execution context service from the service provider.
                PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                // Obtain the tracing service from the service provider.
                TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                // Get the notification service from the service provider.
                NotificationService = (IServiceEndpointNotificationService)serviceProvider.GetService(typeof(IServiceEndpointNotificationService));

                // Obtain the organization factory service from the service provider.
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                // Use the factory to generate the organization service.
                OrganizationService = factory.CreateOrganizationService(PluginExecutionContext.UserId);
            }

            /// <summary>
            /// Writes a trace message to the CRM trace log.
            /// </summary>
            /// <param name="message">Message name to trace.</param>
            internal void Trace(string message) {
                if (string.IsNullOrWhiteSpace(message) || TracingService == null) return;                

                if (PluginExecutionContext == null) {
                    TracingService.Trace(message);
                } else {
                    TracingService.Trace(
                        "{0}, Correlation Id: {1}, Initiating User: {2}",
                        message,
                        PluginExecutionContext.CorrelationId,
                        PluginExecutionContext.InitiatingUserId);
                }
            }
        }

        protected abstract void Execute(LocalPluginContext context);

        /// <summary>This is the method that's actually invoked by CRM when the plugin fires.</summary>
        /// <param name="serviceProvider">The service provider.</param>
        public void Execute(IServiceProvider serviceProvider) {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            var localcontext = new LocalPluginContext(serviceProvider);
            localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Entered {0}.Execute()", GetType().Name));            
            try {
                Execute(localcontext);
                return;
            } catch (FaultException<OrganizationServiceFault> e) {
                localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Exception: {0}", e));
                throw;
            } finally {
                localcontext.Trace(string.Format(CultureInfo.InvariantCulture, "Exiting {0}.Execute()", GetType().Name));
            }
        }    
    }
}