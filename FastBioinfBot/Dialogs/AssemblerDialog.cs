using FastBioinfBot.BioinfToolWrappers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
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
    public class AssemblerDialog : ComponentDialog
    {
        public AssemblerDialog() : base(nameof(AssemblerDialog))
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
                Prompt = MessageFactory.Text("Please upload file with sequences in *.fastq format:")
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
            SequencesAssembler.AssemblySequences(localFileName);
            Activity result = await BuildAssemblingResult(stepContext.Context, cancellationToken);
            return await stepContext.EndDialogAsync(result);
        }

        private static async Task<Activity> BuildAssemblingResult(ITurnContext context, CancellationToken cancellationToken)
        {
            Activity reply = MessageFactory.Text("");

            await context.SendActivityAsync(MessageFactory.Text("Please wait. Sequences assembling can take long time... \r\nThe result will be sent to the chat"));

            string attachmentUri = await Helpers.UploadFileAsync(context, cancellationToken, context.Activity.Id, "assembled_sequences.fasta", "text/html");
            var outAttachment = new Attachment()
            {
                ContentType = "text/plain",
                Name = "Assembled sequences",
                ContentUrl = attachmentUri
            };
            reply.Attachments.Add(outAttachment);
            return reply;
        }
    }
}
