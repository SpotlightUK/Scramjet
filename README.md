# Scramjet

Scramjet is a plugin for Microsoft Dynamics CRM that captures business events (new records, updated records, deleted records) and sends information about these events to an external webhook. It demonstrates one possible mechanism for connecting Dynamics CRM to other systems, such as CQRS-style query stores, event sourced transactional systems or web applications.

## Getting Started 

You'll need:

- A running instance of Microsoft Dynamics CRM. It's been tested with CRM Online 2016; other versions might work.
- A web server that's visible from the internet. If you don't have one of these, I recommend using [ngrok](https://ngrok.com/) to create a secure tunnel to localhost so you can tunnel the HTTP calls from CRM to your workstation.

Deploying Scramjet

- Edit `Scramjet.Deployer\Program.cs` and set `WEBHOOK_URL` to the URL where you're running your SampleWebApp instance - you'll need to include the path to `scramjet.axd` handler.
- Create a file in the `Scramjet.Deployer` project folder called  `connectionstrings.secret` file containing the connection credentials for your CRM instance - this file is excluded from revision control so that you don't accidentally commit your CRM credentials to GitHub.Set this file to 'Copy to Output Directory = Copy Always' It should look like this:

`<connectionStrings>`
`<add name="crm" connectionString="Url=https://my-instance.crm4.dynamics.com;Username=me@my-org.onmicrosoft.com;Password=..." />` 
`</connectionStrings>` 

- Build and run Scramjet.Deployer.exe to compile the plugin and deploy it into your Dynamics CRM instance.
- Edit some entities, and verify that you're getting HTTP notifications. By default it'll bind to the Create, Update and Delete events of the Contract, Contact and Account entities. 

## Components

### Scramjet.CrmPlugins

The plugin assembly that is deployed into your Dynamics CRM instance. Note that this assembly includes a custom **ilmerge** build step which merges `Newtonsoft.Json` into the target assembly and then signs it. This is a limitation of Dynamics CRM, which doesn't support reference assemblies and will only allow signed assemblies to be deployed as plugins.

### Scramjet.Deployer

A console application that will deploy the CrmPlugins assembly into your Dynamics CRM instance, and then register the plugin with specified entity events. This replaces the Plug-in Registration tool included with the Dynamics CRM SDK, and provides a programmatic deployment mechanism suitable for inclusion in a continuous deployment pipeline or automated release process.

### Scramjet.Web

Includes a simple HTTP handler implementation that will translate incoming HTTP notifications into .NET events.

### Scramjet.SampleWebApp

Sample app showing how to incorporate the Scramjet.Web.Handlers.CrmEventHandler into your ASP.NET web application.



