using FastBioinfBot.BioinfToolWrappers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
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
    public class TrimmomaticDialog : ComponentDialog
    {
        public TrimmomaticDialog() : base(nameof(TrimmomaticDialog))
        {
            AddDialog(new ChoicePrompt("ModePromt"));
            AddDialog(new NumberPrompt<int>("LeadingPromt"));
            AddDialog(new NumberPrompt<int>("TrailingPromt"));
            AddDialog(new NumberPrompt<int>("MinLenPromt"));

            AddDialog(new AttachmentPrompt("AttachmentForwardPromt"));
            AddDialog(new AttachmentPrompt("AttachmentReversePromt"));


            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ModeStepAsync,
                LeadingStepAsync,
                TrailingStepAsync,
                MinLenStepAsync,
                AttachmentForwardStepAsync,
                AttachmentReverseStepAsync,
                ProcessDataStepAsync
            }));
            InitialDialogId = "WaterfallDialog";
        }

        private static async Task<DialogTurnResult> ModeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["TrimmomaticParams"] = new TrimmomaticInputParams();
            return await stepContext.PromptAsync("ModePromt",
                new PromptOptions
                {

                    Prompt = MessageFactory.Text("Please choose input data type (default - paired end)"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Paired end", "Single end" }),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> LeadingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var inputParams = (TrimmomaticInputParams)stepContext.Values["TrimmomaticParams"];
            inputParams.Mode = ((FoundChoice)stepContext.Result).Value;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter leading trim size (default: 3)") };

            // Ask the user to enter their age.
            return await stepContext.PromptAsync("LeadingPromt", promptOptions, cancellationToken);
        }

        private static async Task<DialogTurnResult> TrailingStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var inputParams = (TrimmomaticInputParams)stepContext.Values["TrimmomaticParams"];
            inputParams.Leading = (int)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter trailing trim size (default: 3)") };

            // Ask the user to enter their age.
            return await stepContext.PromptAsync("TrailingPromt", promptOptions, cancellationToken);
        }

        private static async Task<DialogTurnResult> MinLenStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var inputParams = (TrimmomaticInputParams)stepContext.Values["TrimmomaticParams"];
            inputParams.Trailing = (int)stepContext.Result;

            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("Please enter min length of reads (default - 32)") };

            // Ask the user to enter their age.
            return await stepContext.PromptAsync("MinLenPromt", promptOptions, cancellationToken);
        }

        private static async Task<DialogTurnResult> AttachmentForwardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var inputParams = (TrimmomaticInputParams)stepContext.Values["TrimmomaticParams"];
            inputParams.MinLen = (int)stepContext.Result;
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please upload forward reads file:")
            };
            return await stepContext.PromptAsync("AttachmentForwardPromt", promptOptions, cancellationToken);
        }

        private static async Task<DialogTurnResult> AttachmentReverseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Attachment attachment = ((List<Attachment>)stepContext.Result)[0];
            stepContext.Values["ForwardAttachment"] = attachment;
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please upload reverse reads file:")
            };
            return await stepContext.PromptAsync("AttachmentForwardPromt", promptOptions, cancellationToken);
        }

        private static async Task<DialogTurnResult> ProcessDataStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Attachment reverseAttachment = ((List<Attachment>)stepContext.Result)[0];
            Attachment forwardAttachment = (Attachment)stepContext.Values["ForwardAttachment"];

            var inputParams = (TrimmomaticInputParams)stepContext.Values["TrimmomaticParams"];

            var forwardLocalFileName = Path.Combine(Path.GetTempPath(), forwardAttachment.Name);
            var reverseLocalFileName = Path.Combine(Path.GetTempPath(), reverseAttachment.Name);

            using (var webClient = new WebClient())
            {
                webClient.DownloadFile(forwardAttachment.ContentUrl, forwardLocalFileName);
                webClient.DownloadFile(reverseAttachment.ContentUrl, reverseLocalFileName);

            }
            inputParams.ForwardReadsFileName = forwardLocalFileName;
            inputParams.ReverseReadsFileName = reverseLocalFileName;

            bool isProcessed = TrimmomaticWrapper.ProcessFilesByTrimmomatic(inputParams);
            if (!isProcessed)
            {
                Activity failReply = MessageFactory.Text($"Trimmomatics fails to process files");
                return await stepContext.EndDialogAsync(failReply);
            }

            Activity result = await BuildTrimmomaticResult(stepContext.Context, cancellationToken);
            return await stepContext.EndDialogAsync(result);
        }

        private static async Task<Activity> BuildTrimmomaticResult(ITurnContext context, CancellationToken cancellationToken)
        {
            Activity reply = MessageFactory.Text("Trimmomatic results");

            
            string attachmentUri = await Helpers.UploadFileAsync(context, cancellationToken, context.Activity.Id, "forward_paired.7z", "application/zip");
            var attachment = new Attachment()
            {
                ContentType = "application/zip",
                Name = "Trimmed forward reads",
                ContentUrl = attachmentUri
            };
            reply.Attachments.Add(attachment);

            if (File.Exists("reverse_paired.7z"))
            {
                attachmentUri = await Helpers.UploadFileAsync(context, cancellationToken, context.Activity.Id, "reverse_paired.7z", "application/zip");
                attachment = new Attachment()
                {
                    ContentType = "application/zip",
                    Name = "Trimmed reverse reads",
                    ContentUrl = attachmentUri
                };
                reply.Attachments.Add(attachment);
            }
            return reply;
        }


    }
}
