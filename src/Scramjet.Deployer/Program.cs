﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Scramjet.CrmPlugins;

namespace Scramjet.Deployer {
    public class Program {

        static OrganizationService crm;

        static void Main(string[] args) {
            crm = new OrganizationService(CrmConnection.Parse(ConfigurationManager.ConnectionStrings["crm"].ConnectionString));
            var name = "Scramjet Notification Plugin";
            var friendlyName = "Notifies external systems of data changes that happen within Dynamics CRM";

            var plugin = crm.RegisterPlugin(typeof(NotifyEntityChangePlugin), name, friendlyName);

            var config = "http://scramjet-ngrok-io-q8ektuhmsgm7.runscope.net/scramjet.axd";
            foreach (var message in new[] { "Create", "Update", "Delete" }) {
                foreach (var entity in new[] { "contact", "account", "contract" }) {
                    var step = crm.RegisterPluginStep(plugin, message, entity, config);
                    Console.WriteLine($"Registered {step.Id} - '{step["name"]}'");
                }
            }
        }
    }
}