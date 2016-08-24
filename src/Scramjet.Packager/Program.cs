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


namespace Scramjet.Packager {

    class Program {

        static void Main(string[] args) {
            var crm = new OrganizationService(CrmConnection.Parse(ConfigurationManager.ConnectionStrings["crm"].ConnectionString));
            var publishAllXmlRequest = new PublishAllXmlRequest();
            crm.Execute(publishAllXmlRequest);

            var export = new ExportSolutionRequest() {
                SolutionName = "Scramjet",
                Managed = true
            };
            var response = (ExportSolutionResponse)crm.Execute(export);
            File.WriteAllBytes(@"D:\managed.zip", response.ExportSolutionFile);
            Console.WriteLine("Done!");
            Console.ReadKey(false);

        }
    }
}
