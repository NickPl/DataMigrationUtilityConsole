using Microsoft.Xrm.Tooling.Dmt.DataMigCommon.Utility;
using Microsoft.Xrm.Tooling.Dmt.ImportProcessor.DataInteraction;
using System;
using System.Collections.Generic;

namespace DataMigrationUtility
{
    internal class Program
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="args">Connection string, zip file path, threads</param>
        private static void Main(string[] args)
        {
            Console.WriteLine("DataMigrationUtility 1.0");

            if (args.Length == 0)
            {
                Console.WriteLine("To use: [ConnectionString] [DataZipPath] [ThreadsNumber(5)]");
                Console.WriteLine("ex: \"ServiceUri=http://myserver/CRM;\" \"data.zip\" 5");
                return;
            }

            // ex: "ServiceUri=http://myserver/CRM;"
            string connectionString = args[0];

            // ex: parameters_data.zip
            string workingImportFolder = args[1];

            // ex: 0, 5, 10
            int threads = 5;
            if (args.Length > 2)
            {
                threads = int.Parse(args[2]);
            }

            var parser = new ImportCrmDataHandler();
            parser.CrmConnection = new Microsoft.Xrm.Tooling.Connector.CrmServiceClient(connectionString);

            // more connections can be added for multi-threading
            var _importConnections = new Dictionary<int, Microsoft.Xrm.Tooling.Connector.CrmServiceClient>();
            for (int i = 1; i < threads; i++)
            {
                _importConnections.Add(i, new Microsoft.Xrm.Tooling.Connector.CrmServiceClient(connectionString));
            }

            parser.ImportConnections = _importConnections;

            parser.AddNewProgressItem += new EventHandler<ProgressItemEventArgs>(_parser_AddNewProgressItem);
            parser.UpdateProgressItem += new EventHandler<ProgressItemEventArgs>(_parser_UpdateProgressItem);
            parser.UserMappingRequired += new EventHandler<UserMapRequiredEventArgs>(_parser_UserMappingRequired);

            if (!ImportCrmDataHandler.CrackZipFileAndCheckContents(workingImportFolder, workingImportFolder, out workingImportFolder))
            {
                Console.WriteLine("The zip file validation failed.");
            }
            else
            {
                parser.ValidateSchemaFile(workingImportFolder);
                parser.ImportDataToCrm(workingImportFolder, false);
            }
        }

        private static void _parser_UserMappingRequired(object sender, UserMapRequiredEventArgs e)
        {
            Console.WriteLine(e.SystemUsersToImport.ToString());
        }

        private static void _parser_UpdateProgressItem(object sender, ProgressItemEventArgs e)
        {
            Console.WriteLine(e.progressItem.ItemText);
        }

        private static void _parser_AddNewProgressItem(object sender, ProgressItemEventArgs e)
        {
            Console.WriteLine(e.progressItem.ItemText);
        }
    }
}