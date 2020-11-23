using FastBioinfBot.BioinfToolWrappers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FastBioinfBot.Dialogs
{
    public class BlastDialog : CancelAndHelpDialog
    {
        public BlastDialog() : base(nameof(BlastDialog))
        {
            AddDialog(new AttachmentPrompt("AttachmentPromt"));
            AddDialog(new ChoicePrompt("LogModePromt"));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                ModeStepAsync,
                SequenceStepAsync,
                ProcessDataStepAsync
            }));
            InitialDialogId = "WaterfallDialog";
        }

        private static async Task<DialogTurnResult> ModeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("LogModePromt",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("BLAST search can take a long time (>15 minutes). If you want to receive messages about the current progress every 30 seconds, choose 'Log mode'"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Log mode", "Quiet mode" }),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> SequenceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            stepContext.Values["LogMode"] = ((FoundChoice)stepContext.Result).Value;
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please upload your DNA sequence in *.txt to search in NCBI database:")
            };
            return await stepContext.PromptAsync("AttachmentPromt", promptOptions, cancellationToken);
        }

        private static void LogFromBlast(string message, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if(message.Length<400 && !message.Contains("Waiting 2 seconds")&&!message.Contains("Checking on request"))
            {
                var response = MessageFactory.Text(message);
                turnContext.SendActivityAsync(response, cancellationToken);
            }
        }
        private static async Task<DialogTurnResult> ProcessDataStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Attachment attachment = ((List<Attachment>)stepContext.Result)[0];
            string sequence = "";

            using (var webClient = new WebClient())
            {
                sequence = webClient.DownloadString(attachment.ContentUrl);
            }
            var wrapper = new BLASTWrapper();
            if ((string)stepContext.Values["LogMode"] == "Log mode")
            {
                Action<string> logMessage = delegate (string s) { LogFromBlast(s, stepContext.Context, cancellationToken); };
                wrapper.Log = logMessage;
            }

            string result = await wrapper.SearchBlast(sequence, cancellationToken);
            Activity reply = MessageFactory.Text(result);
            return await stepContext.EndDialogAsync(reply, cancellationToken);
        }
    }
}
