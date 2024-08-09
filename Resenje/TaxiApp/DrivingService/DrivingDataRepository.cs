﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
namespace DrivingService
{
    public class DrivingDataRepository
    {

        private CloudStorageAccount cloudAcc;

        private CloudTableClient tableClient;
        private CloudTable _trips;

        public DrivingDataRepository(string tableName)
        {
            try
            {

                string dataConnectionString = Environment.GetEnvironmentVariable("DataConnectionString");
                CloudAcc = CloudStorageAccount.Parse(dataConnectionString); // create cloud client for making blob,table or queue 


                TableClient = CloudAcc.CreateCloudTableClient(); // table client

                Trips = TableClient.GetTableReference(tableName); // create if not exists Users table 
                Trips.CreateIfNotExistsAsync().Wait();

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public CloudStorageAccount CloudAcc { get => cloudAcc; set => cloudAcc = value; }
        public CloudTableClient TableClient { get => tableClient; set => tableClient = value; }
        public CloudTable Trips { get => _trips; set => _trips = value; }
    }
}
