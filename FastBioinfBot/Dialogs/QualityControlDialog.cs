using FastBioinfBot.BioinfToolWrappers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FastBioinfBot.Dialogs
{
    public class QualityControlDialog : ComponentDialog
    {
        public QualityControlDialog():base(nameof(QualityControlDialog))
        {
            AddDialog(new AttachmentPrompt("Attachment1Promt"));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AttachmentStepAsync,
                ProcessDataStepAsync
            })); 
            InitialDialogId = "WaterfallDialog";
        }

        private static async Task<DialogTurnResult> AttachmentStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions 
            {
                Prompt = MessageFactory.Text("Please upload file for FastQC processing:")
            };
            return await stepContext.PromptAsync("Attachment1Promt", promptOptions, cancellationToken);
        }

        private static async Task<DialogTurnResult> ProcessDataStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            Attachment attachment = ((List<Attachment>)stepContext.Result)[0];
            var remoteFileUrl = attachment.ContentUrl;
            var localFileName = Path.Combine(Path.GetTempPath(), attachment.Name);
            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(remoteFileUrl, localFileName);
            }
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please wait. FastQC processing your file..."));

            bool isProcessed = FastQCWrapper.ProcessFastqFile(localFileName, out string allResultsFileName, out string htmlResultsFileName, out string errorOutput);
            if (!isProcessed)
            {
                Activity failReply = MessageFactory.Text($"FastQC fails to process file {localFileName}. {Environment.NewLine}" +
                    $"Error std output: {errorOutput}");
                return await stepContext.EndDialogAsync(failReply);
            }

            Activity result = await BuildFastQCResult(stepContext.Context, cancellationToken, htmlResultsFileName, allResultsFileName);
            return await stepContext.EndDialogAsync(result);
        }

        private static async Task<Activity> BuildFastQCResult(ITurnContext context, CancellationToken cancellationToken, string htmlResultsFileName, string zipResultsFileName)
        {
            Activity reply = MessageFactory.Text("FastQC results");

            string attachmentUri = await Helpers.UploadFileAsync(context, cancellationToken, context.Activity.Id, htmlResultsFileName, "text/html");
            var outAttachment = new Attachment()
            {
                ContentType = "text/html",
                Name = "Quality control HTML report",
                ContentUrl = attachmentUri
            };
            reply.Attachments.Add(outAttachment);

            attachmentUri = await Helpers.UploadFileAsync(context, cancellationToken, context.Activity.Id, zipResultsFileName, "application/zip");
            var outAttachmentZip = new Attachment()
            {
                ContentType = "application/zip",
                Name = "All FastQC results in ZIP",
                ContentUrl = attachmentUri
            };
            reply.Attachments.Add(outAttachmentZip);

            return reply;
        }


    }
}
