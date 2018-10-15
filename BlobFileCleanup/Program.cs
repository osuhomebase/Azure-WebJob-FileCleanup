using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;

namespace BlobFileCleanup
{

    //-----------------------------------------------------------------
    // This code is an adaptation of this article:
    // https://www.surinderbhomra.com/Blog/Post/2017/04/23/Azure-WebJob-To-Delete-Old-Files-from-A-Blob-Container
    // Surinder Bhomra does an excellent job explaining step by step 
    // Required:  Update Nuget Packages for 
    //           WindowsAzure.Storage
    //           Microsoft.Azure.KeyVault.Core
    //      Add Reference to:
    //           System.Configuration
    // Be sure to add Connection Strings to your App.Config
    // ----------------------------------------------------------------

    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost(config);
            // The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer dataContainer = blobClient.GetContainerReference(ConfigurationManager.AppSettings["Azure.ContainerName"]);

                Console.WriteLine("Hourly threshold to remove records: {0}", ConfigurationManager.AppSettings["Azure.CleanupHours"]);

                #region Retrieve all data items greater than 24 hours and delete them

                Console.WriteLine("Retrieving old data files...");

                // Get files where the "Last Modified Date" is olders than 24 hours.
                IEnumerable<CloudBlob> oldData = dataContainer.ListBlobs()
                                .OfType<CloudBlob>()
                                .Where(b => b.Properties.LastModified.Value.Date < DateTime.Now.AddHours(int.Parse(ConfigurationManager.AppSettings["Azure.CleanupHours"].ToString()) * -1));

                IList<CloudBlob> dataBlobs = oldData as IList<CloudBlob> ?? oldData.ToList();

                Console.WriteLine("Data records retrieved: {0}.", dataBlobs.Count);
                Console.WriteLine("Removing old data files...");

                // Loop through the files and delete if they exist.
                foreach (CloudBlob dataBlob in dataBlobs)
                {
                    bool isDeleted = dataBlob.DeleteIfExists();

                    if (isDeleted)
                        Console.WriteLine("Deleted: {0}.", dataBlob.Name);
                }

                #endregion

                Console.WriteLine("Removing old data complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cleaning container files: {0}", ex.Message);
            }

            Console.WriteLine("Clean Containers WebJob complete.");
        }
    }
    class StorageUnit
    {
        public string containerName { get; set; }
        public string connectionString { get; set; }
        public string cleanupHours { get; set; }
    }
}
