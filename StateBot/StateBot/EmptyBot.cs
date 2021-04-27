// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateBot
{
    public class EmptyBot : ActivityHandler
    {

        private readonly UserState _userstate;
        private readonly ConversationState _conversationstate;

        public EmptyBot(UserState userState, ConversationState conversationState)

        {
            _userstate = userState;
            _conversationstate = conversationState;

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello world!"), cancellationToken);
                }
            }
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var ConversationStateAccessor = _conversationstate.CreateProperty<ConversationData>(nameof(ConversationData));
            var conversationData = await ConversationStateAccessor.GetAsync(turnContext, () => new ConversationData());

            var UserStateAccessor = _userstate.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await UserStateAccessor.GetAsync(turnContext, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                if (conversationData.PromptUserName)
                {


                    userProfile.Name = turnContext.Activity.Text?.Trim();
                    await turnContext.SendActivityAsync(MessageFactory.Text(string.Format("Hi. {0}", userProfile.Name)));
                    conversationData.PromptUserName = false;
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Whats Your Name?"));
                    conversationData.PromptUserName = true;
                }
                
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("Have a Good Day..."));
                await turnContext.SendActivityAsync(MessageFactory.Text("Last Message Details are:"));
                var MessageTime = (DateTimeOffset)turnContext.Activity.Timestamp;
                var LocalTime = MessageTime.ToLocalTime();
                conversationData.TStamp = LocalTime.ToString();
                conversationData.ChannelId = turnContext.Activity.ChannelId.ToString();
                await turnContext.SendActivityAsync(MessageFactory.Text(string.Format("Received At: {0}", conversationData.TStamp)));
                await turnContext.SendActivityAsync(MessageFactory.Text(string.Format("Channel Id: {0}", conversationData.ChannelId)));


            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _conversationstate.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userstate.SaveChangesAsync(turnContext, false, cancellationToken);


        }
    }
}
