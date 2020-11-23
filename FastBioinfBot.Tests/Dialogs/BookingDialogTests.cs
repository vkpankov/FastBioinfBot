// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using FastBioinfBot;
using FastBioinfBot.Dialogs;
using FastBioinfBot.Tests.Common;
using FastBioinfBot.Tests.Dialogs.TestData;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Testing;
using Microsoft.Bot.Builder.Testing.XUnit;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace FastBioinfBot.Tests.Dialogs
{
    public class BookingDialogTests : BotTestBase
    {
        private readonly IMiddleware[] _middlewares;

        public BookingDialogTests(ITestOutputHelper output)
            : base(output)
        {
            _middlewares = new IMiddleware[] { new XUnitDialogTestLogger(output) };
        }

    }
}
