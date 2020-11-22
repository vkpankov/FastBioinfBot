using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace FastBioinfBot
{
    public static class Helpers
    {
        public static async Task<string> UploadFileAsync(ITurnContext turnContext, CancellationToken cancellationToken, string conversationId, string fileName, string contentType)
        {
            var connector = turnContext.TurnState.Get<IConnectorClient>() as ConnectorClient;
            var attachments = new Attachments(connector);
            byte[] bytesContent = File.ReadAllBytes(fileName);

            var response = await attachments.Client.Conversations.UploadAttachmentAsync(
                conversationId,
                new AttachmentData
                {

                    Name = Path.GetFileName(fileName),
                    OriginalBase64 = bytesContent,
                    Type = contentType,
                },
                cancellationToken);

            var attachmentUri = attachments.GetAttachmentUri(response.Id, Path.GetFileName(fileName));
            return attachmentUri;
        }
    }
}
