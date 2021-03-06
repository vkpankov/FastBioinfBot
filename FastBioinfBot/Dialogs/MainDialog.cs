// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.10.3

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FastBioinfBot.Dialogs
{
    public class MainDialog : CancelAndHelpDialog
    {
        private readonly UserState _userState;

        public MainDialog(UserState userState)
            : base(nameof(MainDialog))
        {
            _userState = userState;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new QualityControlDialog());
            AddDialog(new TrimmomaticDialog());
            AddDialog(new BlastDialog());
            AddDialog(new AssemblerDialog());

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SendSuggestedActionsAsync,
                InitialStepAsync,
                FinalStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> SendSuggestedActionsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {

                    Prompt = MessageFactory.Text("Please choose bioinformatics tool\r\n" +
                    "Use FastQC to do some quality control checks on raw sequence data\r\n" +
                    "Use Trimmomatic to  perform trimming for illumina paired-end and single ended data\r\n" +
                    "Use NCBI BLAST to find you sequence in NCBI nucleotides database\r\n" +
                    "Use 'Build assembly' to build contigs (large sequences) from reads"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "FastQC", "Trimmomatic", "NCBI BLAST", "Build assembly" }),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string selectedTool = ((FoundChoice)stepContext.Result).Value;
            switch (selectedTool)
            {
                case "FastQC":
                    {
                        return await stepContext.BeginDialogAsync(nameof(QualityControlDialog), null, cancellationToken);
                    }
                case "Trimmomatic":
                    {
                        return await stepContext.BeginDialogAsync(nameof(TrimmomaticDialog), null, cancellationToken);
                    }
                case "NCBI BLAST":
                    {
                        return await stepContext.BeginDialogAsync(nameof(BlastDialog), null, cancellationToken);
                    }
                case "Build assembly":
                    {
                        return await stepContext.BeginDialogAsync(nameof(AssemblerDialog), null, cancellationToken);
                    }
                default:
                    {
                        return await SendSuggestedActionsAsync(stepContext, cancellationToken);
                    }
            }


        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var info = stepContext.Result;
            await stepContext.Context.SendActivityAsync((IActivity)info);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
