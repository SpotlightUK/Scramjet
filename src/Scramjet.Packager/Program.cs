using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Configuration;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Scramjet.CrmPlugins;
using EntityExtensions = Scramjet.CrmPlugins.EntityExtensions;


namespace Scramjet.Packager {

    public static class CrmAssemblySourceType {
        public static readonly OptionSetValue Database = new OptionSetValue(0);
        public static readonly OptionSetValue Disk = new OptionSetValue(1);
        public static readonly OptionSetValue GAC = new OptionSetValue(2);
    }

    public static class CrmAssemblyIsolationMode {
        public static readonly OptionSetValue None = new OptionSetValue(1);
        public static readonly OptionSetValue Sandbox = new OptionSetValue(2);
    }


    public static class CrmPluginStage {
        public static readonly OptionSetValue PreValidation = new OptionSetValue(10);
        public static readonly OptionSetValue PreOperation = new OptionSetValue(20);
        public static readonly OptionSetValue PostOperation = new OptionSetValue(40);
    }

    public enum CrmPluginStepMode {
        Asynchronous = 1,
        Synchronous = 0
    }

    public enum CrmPluginStepStage {
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40,
        PostOperationDeprecated = 50
    }

    public enum CrmPluginStepDeployment {
        ServerOnly = 0,
        OfflineOnly = 1,
        Both = 2
    }

    public enum CrmPluginStepInvocationSource {
        Parent = 0,
        Child = 1
    }

    public class Program {
        static OrganizationService crm;

        static void Main(string[] args) {
            crm = new OrganizationService(CrmConnection.Parse(ConfigurationManager.ConnectionStrings["crm"].ConnectionString));


            //var sq = new QueryByAttribute {
            //    EntityName = "sdkmessageprocessingstep",
            //    ColumnSet = new ColumnSet { AllColumns = true }
            //};
            //sq.Attributes.Add("name");
            //sq.Values.Add("Scramjet.CrmPlugins.NotifyEntityChangePlugin: Update of contact");
            //var result = crm.RetrieveMultiple(sq);
            //var messages = result.Entities;
            //File.WriteAllText(@"d:\sdkmessagefilter.csv", "name\tid" + Environment.NewLine);
            //foreach (var m in messages) {
            //    File.AppendAllText(@"d:\sdkmessagefilter.csv", m.Attributes["name"] + "\t" + m.Id + Environment.NewLine);
            //    Console.WriteLine(m.Id);
            //    foreach (var a in m.Attributes) {
            //        File.AppendAllText(@"d:\sdkmessagefilter.csv", "    " + a.Key + " : " + EntityExtensions.Flatten(a.Value));
            //        Console.WriteLine("    " + a.Key + " : " + EntityExtensions.Flatten(a.Value));
            //    }
            //}
            //Console.WriteLine("Done");
            //Console.ReadKey(false);

            //var publishAllXmlRequest = new PublishAllXmlRequest();
            //crm.Execute(publishAllXmlRequest);

            //var export = new ExportSolutionRequest() {
            //    SolutionName = "Scramjet",
            //    Managed = true
            //};

            //var response = (ExportSolutionResponse)crm.Execute(export);
            //File.WriteAllBytes(@"D:\managed.zip", response.ExportSolutionFile);
            var assembly = typeof(NotifyEntityChangePlugin).Assembly;
            string[] props = assembly.GetName()
                .FullName.Split(",=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            Entity pluginAssembly = new Entity("pluginassembly");
            pluginAssembly["name"] = props[0];
            pluginAssembly["culture"] = props[4];
            pluginAssembly["version"] = props[2];
            pluginAssembly["publickeytoken"] = props[6];
            pluginAssembly["sourcetype"] = CrmAssemblySourceType.Database;
            pluginAssembly["isolationmode"] = CrmAssemblyIsolationMode.Sandbox;
            pluginAssembly["content"] = Convert.ToBase64String(File.ReadAllBytes(assembly.Location));

            var q = new QueryExpression() {
                EntityName = "pluginassembly",
                Criteria = new FilterExpression() {
                    Conditions = { new ConditionExpression("name", ConditionOperator.Equal, props[0]) }
                }
            };

            var results = crm.RetrieveMultiple(q);
            var existingAssembly = results.Entities.FirstOrDefault();
            Guid assemblyId;
            if (existingAssembly != null) {
                pluginAssembly.Id = assemblyId = existingAssembly.Id;
                crm.Update(pluginAssembly);
            } else {
                assemblyId = crm.Create(pluginAssembly);
            }

            Entity pluginType = new Entity("plugintype");
            pluginType["pluginassemblyid"] = new EntityReference("pluginassembly", assemblyId);
            pluginType["typename"] = "Scramjet.CrmPlugins.NotifyEntityChangePlugin";
            pluginType["friendlyname"] = "Scramjet CRM Plugin";
            pluginType["description"] = "Scramjet CRM Plugin Description";

            var pq = new QueryExpression() {
                EntityName = "plugintype",
                Criteria = new FilterExpression() {
                    FilterOperator = LogicalOperator.And,
                    Conditions = {
                        new ConditionExpression("pluginassemblyid", ConditionOperator.Equal, assemblyId),
                        new ConditionExpression("typename", ConditionOperator.Equal,  "Scramjet.CrmPlugins.NotifyEntityChangePlugin"),
                    }
                }
            };

            var existingPluginType = crm.RetrieveMultiple(pq).Entities.FirstOrDefault();
            Guid pluginTypeId;
            if (existingPluginType != null) {
                pluginType.Id = pluginTypeId = existingPluginType.Id;
                crm.Update(pluginType);
            } else {
                pluginTypeId = crm.Create(pluginType);
            }

            var sdkMessage = FindSdkMessageByName("Update");
            var filter = FindOrCreateSdkMessageFilter("contact", sdkMessage);

            var step = new Entity("sdkmessageprocessingstep");
            step["name"] = "Notify on Update of Contact";
            step["rank"] = 1;
            step["stage"] = new OptionSetValue(40);
            step["supporteddeployment"] = new OptionSetValue(0);
            step["invocationsource"] = new OptionSetValue(1); // What does this signify? I do not know!

            step["plugintypeid"] = new EntityReference("plugintype", pluginTypeId);
            step["sdkmessageid"] = new EntityReference("sdkmessage", Guid.Parse("20bebb1b-ea3e-db11-86a7-000a3a5473e8")); //  Guid.Parse("9ebdbb1b-ea3e-db11-86a7-000a3a5473e8")));
            step["mode"] = new OptionSetValue(1); // don't know... ?
            step["configuration"] = "look at all these bees!";
            step["sdkmessagefilterid"] = new EntityReference("sdkmessagefilter", filter.Id);

            var stepId = crm.Create(step);
            Console.WriteLine(stepId);
            Console.WriteLine("Done!");
            Console.ReadKey(false);

        }

        private static Entity FindSdkMessageByName(string name) {
            var pluginTypeQuery = new QueryByAttribute {
                EntityName = "sdkmessage",
                ColumnSet = new ColumnSet { AllColumns = true }
            };
            pluginTypeQuery.Attributes.Add("name");
            pluginTypeQuery.Values.Add(name);
            return (crm.RetrieveMultiple(pluginTypeQuery).Entities.FirstOrDefault());

        }
        private static Entity FindOrCreateSdkMessageFilter(string entity, Entity sdkMessage) {
            if (sdkMessage != null) {
                var sdkMessageId = sdkMessage.Id;
                var sdkMessageFilterQuery = new QueryByAttribute {
                    EntityName = "sdkmessagefilter",
                    ColumnSet = new ColumnSet { AllColumns = true }
                };
                sdkMessageFilterQuery.Attributes.Add("primaryobjecttypecode");
                sdkMessageFilterQuery.Values.Add(entity);
                sdkMessageFilterQuery.Attributes.Add("sdkmessageid");
                sdkMessageFilterQuery.Values.Add(sdkMessageId);
                var sdkMessageFilter = crm.RetrieveMultiple(sdkMessageFilterQuery).Entities.FirstOrDefault();
                return (sdkMessageFilter);
            }
            return (null);
        }
    }
}
