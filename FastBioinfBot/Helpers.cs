using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Azure.Storage.Blobs;

namespace FastBioinfBot
{
    public static class Helpers
    {
        private static readonly string connectionString = "DefaultEndpointsProtocol=https;AccountName=bioinfbotstorage;AccountKey=sRS+UBWKb9CTR3HbkjfSlsWM79LItMYkRTxs+fFbb2ytUT+5vxyweU2bRMua0AOsaqxj7NdOWTWax3keyEREdQ==;EndpointSuffix=core.windows.net";

        public static async Task<string> UploadFileAsync(ITurnContext turnContext, CancellationToken cancellationToken, string conversationId, string fileName, string contentType)
        {



            var connector = turnContext.TurnState.Get<IConnectorClient>() as ConnectorClient;
    
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            //Create a unique name for the container
            string containerName = "results" + Guid.NewGuid().ToString();

            // Create the container and return a container client object
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName, Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            BlobClient blobClient = containerClient.GetBlobClient(Path.GetFileName(fileName));

            // Open the file and upload its data
            using FileStream uploadFileStream = File.OpenRead(fileName);
            await blobClient.UploadAsync(uploadFileStream, true, cancellationToken);
            uploadFileStream.Close();

            return blobClient.Uri.ToString();
        }
    }
}
