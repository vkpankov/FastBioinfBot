// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Bio;
using Bio.Web.Blast;
using FastBioinfBot.Bots;
using FastBioinfBot.Tests.Common;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FastBioinfBot.Tests.Bots
{
    public class DialogAndWelcomeBotTests
    {
        public void Log(string log)
        {

        }
        [Fact]
        public async Task ReturnsWelcomeCardOnConversationUpdate()
        {

            NcbiBlastWebHandler handler = new NcbiBlastWebHandler()
            {
                EndPoint = "https://www.ncbi.nlm.nih.gov/blast/Blast.cgi",
                TimeoutInSeconds = 60000,
                LogOutput = Log
            };
            Sequence sequence = new Sequence(DnaAlphabet.Instance, "CCTGGAAAAGGGCTTGAGTGGGTGGGAGGTTTTGATCCTGAACATGGTACAACAATCTAC");
            List<Bio.ISequence> sequences = new List<Bio.ISequence>();
            sequences.Append(sequence);
            var request = new BlastRequestParameters(sequences);
            request.Database = "nt";
            request.Program = BlastProgram.Blastn;
            request.Sequences.Add(sequence);
            HttpContent result = handler.BuildRequest(request);
            var executeResult = handler.ExecuteAsync(request, CancellationToken.None).Result;
            Bio.Web.Blast.BlastXmlParser parser = new BlastXmlParser();
            var results = parser.Parse(executeResult).ToList();


            // Arrange
            var mockRootDialog = SimpleMockFactory.CreateMockDialog<Dialog>(null, "mockRootDialog");
            var memoryStorage = new MemoryStorage();
            var sut = new DialogAndWelcomeBot<Dialog>(new ConversationState(memoryStorage), new UserState(memoryStorage), mockRootDialog.Object, null);

            // Create conversationUpdate activity
            var conversationUpdateActivity = new Activity
            {
                Type = ActivityTypes.ConversationUpdate,
                MembersAdded = new List<ChannelAccount>
                {
                    new ChannelAccount { Id = "theUser" },
                },
                Recipient = new ChannelAccount { Id = "theBot" },
            };
            var testAdapter = new TestAdapter(Channels.Test);

            // Act
            // Send the conversation update activity to the bot.
            await testAdapter.ProcessActivityAsync(conversationUpdateActivity, sut.OnTurnAsync, CancellationToken.None);

            // Assert we got the welcome card
            var reply = (IMessageActivity)testAdapter.GetNextReply();
            Assert.Equal(1, reply.Attachments.Count);
            Assert.Equal("application/vnd.microsoft.card.adaptive", reply.Attachments.FirstOrDefault()?.ContentType);

            // Assert that we started the main dialog.
            reply = (IMessageActivity)testAdapter.GetNextReply();
            Assert.Equal("Dialog mock invoked", reply.Text);
        }
    }
}
