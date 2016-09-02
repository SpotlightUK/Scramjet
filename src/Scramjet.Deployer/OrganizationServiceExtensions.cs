using System.IO;
using System.Reflection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;

namespace Scramjet.Deployer {
    public static class OrganizationServiceExtensions {


        public static void ExportSolution(this OrganizationService crm, string solutionName, string exportFileName, SolutionExportType exportType) {
            crm.Execute(new PublishAllXmlRequest());
            var export = new ExportSolutionRequest() {
                SolutionName = solutionName,
                Managed = (exportType == SolutionExportType.Managed)
            };
            var response = (ExportSolutionResponse)crm.Execute(export);
            File.WriteAllBytes(exportFileName, response.ExportSolutionFile);
        }

        private static Entity RegisterPluginAssembly(this OrganizationService crm, Assembly assembly) {
            var assemblyName = assembly.GetName();

            var query = new QueryByAttribute("pluginassembly");
            query.Attributes.Add("name");
            query.Values.Add(assemblyName.Name);
            var results = crm.RetrieveMultiple(query);

            var pluginAssembly = results.Entities.FirstOrDefault() ?? new Entity("pluginassembly");

            pluginAssembly["name"] = assemblyName.Name;
            pluginAssembly["culture"] = assemblyName.CultureName;
            pluginAssembly["version"] = assemblyName.Version.ToString();
            pluginAssembly["publickeytoken"] = String.Concat(Array.ConvertAll(assemblyName.GetPublicKeyToken(), b => b.ToString("x2")));

            pluginAssembly["sourcetype"] = new OptionSetValue((int)CrmAssemblySourceType.Database);
            pluginAssembly["isolationmode"] = new OptionSetValue((int)CrmAssemblyIsolationMode.Sandbox);
            pluginAssembly["content"] = Convert.ToBase64String(File.ReadAllBytes(assembly.Location));

            if (pluginAssembly.Id != Guid.Empty) {
                crm.Update(pluginAssembly);
            } else {
                pluginAssembly.Id = crm.Create(pluginAssembly);
            }
            return (pluginAssembly);
        }

        public static Entity RegisterPlugin(this OrganizationService crm, Type type, string friendlyName, string description) {
            var assembly = RegisterPluginAssembly(crm, type.Assembly);
            var pq = new QueryExpression() {
                EntityName = "plugintype",
                Criteria = new FilterExpression() {
                    FilterOperator = LogicalOperator.And,
                    Conditions = {
                        new ConditionExpression("pluginassemblyid", ConditionOperator.Equal, assembly.Id),
                        new ConditionExpression("typename", ConditionOperator.Equal,  type.FullName),
                    }
                }
            };

            var pluginType = crm.RetrieveMultiple(pq).Entities.FirstOrDefault() ?? new Entity("plugintype");
            pluginType["pluginassemblyid"] = new EntityReference("pluginassembly", assembly.Id);
            pluginType["typename"] = type.FullName;
            pluginType["friendlyname"] = friendlyName;
            pluginType["description"] = description;

            if (pluginType.Id != Guid.Empty) {
                crm.Update(pluginType);
            } else {
                pluginType.Id = crm.Create(pluginType);
            }
            return (pluginType);
        }

        public static Entity RegisterPluginStep(this OrganizationService crm, Entity pluginType, string message, string entity, string config) {

            var sdkMessage = crm.FindSdkMessageByName(message);
            var filter = crm.FindSdkMessageFilter(entity, sdkMessage);

            var step = new Entity("sdkmessageprocessingstep") {
                ["plugintypeid"] = new EntityReference("plugintype", pluginType.Id),
                ["sdkmessageid"] = new EntityReference("sdkmessage", sdkMessage.Id)
            };

            if (filter != null) step["sdkmessagefilterid"] = new EntityReference("sdkmessagefilter", filter.Id);

            var q2 = new QueryByAttribute("sdkmessageprocessingstep");
            foreach (var a in step.Attributes) {
                q2.Attributes.Add(a.Key);
                q2.Values.Add(a.Value is EntityReference ? ((EntityReference)a.Value).Id : a.Value);
            }
            var results = crm.RetrieveMultiple(q2);
            if (results.Entities.Any()) {
                step = results.Entities.First();
            }

            step["name"] = $"Fire notification on {message} of {entity}";
            step["rank"] = 1;
            step["stage"] = new OptionSetValue((int)CrmPluginStepStage.PostOperation);
            step["supporteddeployment"] = new OptionSetValue((int)CrmPluginStepDeployment.ServerOnly);
            step["mode"] = new OptionSetValue((int)CrmPluginStepMode.Asynchronous); // don't know... ?
            step["asyncautodelete"] = true;
            step["configuration"] = config;

            if (step.Id != Guid.Empty) {
                crm.Update(step);
            } else {
                step.Id = crm.Create(step);
            }
            return (step);

        }

        private static Entity FindSdkMessageByName(this OrganizationService crm, string name) {
            var pluginTypeQuery = new QueryByAttribute {
                EntityName = "sdkmessage",
                ColumnSet = new ColumnSet { AllColumns = true }
            };
            pluginTypeQuery.Attributes.Add("name");
            pluginTypeQuery.Values.Add(name);
            return (crm.RetrieveMultiple(pluginTypeQuery).Entities.FirstOrDefault());

        }

        private static Entity FindSdkMessageFilter(this OrganizationService crm, string entity, Entity sdkMessage) {
            if (entity == null || sdkMessage == null) return (null);
            var sdkMessageId = sdkMessage.Id;
            var sdkMessageFilterQuery = new QueryByAttribute {
                EntityName = "sdkmessagefilter",
                ColumnSet = new ColumnSet { AllColumns = true }
            };
            sdkMessageFilterQuery.Attributes.Add("primaryobjecttypecode");
            sdkMessageFilterQuery.Values.Add(entity);
            sdkMessageFilterQuery.Attributes.Add("sdkmessageid");
            sdkMessageFilterQuery.Values.Add(sdkMessageId);
            return crm.RetrieveMultiple(sdkMessageFilterQuery).Entities.FirstOrDefault();
        }

        private enum CrmAssemblySourceType {
            Database = 0,
            Disk = 1,
            GAC = 2
        }

        private enum CrmAssemblyIsolationMode {
            None = 1,
            Sandbox = 2
        }

        private enum CrmPluginStepStage {
            PreValidation = 10,
            PreOperation = 20,
            PostOperation = 40,
        }

        private enum CrmPluginStepMode {
            Synchronous = 0,
            Asynchronous = 1
        }

        private enum CrmPluginStepDeployment {
            ServerOnly = 0,
            OfflineOnly = 1,
            Both = 2
        }
    }
}