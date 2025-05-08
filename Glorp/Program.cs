using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables("GLORP_");

builder.Services.AddDiscordGateway(static opts =>
    {
        opts.Intents = GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent;
    })
    .AddGatewayEventHandler<GlorpHandler>();

var host = builder.Build();

host.UseGatewayEventHandlers();

await host.RunAsync();

[GatewayEvent(nameof(GatewayClient.Ready))]
[GatewayEvent(nameof(GatewayClient.MessageCreate))]
public sealed class GlorpHandler(ILogger<GlorpHandler> logger) : IGatewayEventHandler<ReadyEventArgs>, IGatewayEventHandler<Message>
{
    private ulong _botId = 0;

    public ValueTask HandleAsync(ReadyEventArgs arg)
    {
        logger.LogInformation("Connected to Discord. Bot ID is {Id}.", arg.User.Id);
        _botId = arg.User.Id;
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask HandleAsync(Message arg)
    {
        try
        {
            if (_botId == 0)
            {
                logger.LogWarning("Bot ID has not been set.");
                return;
            }

            if (arg.Author.IsBot)
            {
                logger.LogInformation("Message {Id} ignored because it is from a bot.", arg.Id);
                return;
            }

            if (arg.Author.IsSystemUser is true)
            {
                logger.LogInformation("Message {Id} ignored because it is a system user.", arg.Id);
                return;
            }

            if (arg.MentionedUsers.All(x => x.Id != _botId))
            {
                logger.LogInformation("Message {Id} ignored because it has no bot mention.", arg.Id);
                return;
            }

            await arg.ReplyAsync(new ReplyMessageProperties
            {
                Content = s_replies[Random.Shared.Next(0, s_replies.Length)],
                AllowedMentions = new AllowedMentionsProperties // Don't let anyone be mentioned.
                {
                    Everyone = false,
                    ReplyMention = false,
                    AllowedRoles = [],
                    AllowedUsers = [],
                },
                Nonce = DateTime.UtcNow.Ticks, // Use UTC because of DST.
                FailIfNotExists = false, // Don't fail if the original message got deleted.
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reply to message {Id}.", arg.Id);
        }
    }

    private static readonly ImmutableArray<string> s_replies = [
        @"Yes",
        @"No",
        @"Maybe",
        @"Perhaps",
        @"Perchance",
        @"Idk",
        @"Idfk",
        @"Why ask me?",
        @"What do I look like, Google?",
        @"Bro what?",
        @"Who knows",
        @"That sure is... a question",
        @"wut",
        @"._.",
        @"¯\\\_(ツ)\_/¯",
        @"Wait a minute... who are you?",
        @"Try again later",
        @"Input invalid",
        @"ERROR 404: SOLUTION NOT FOUND",
        @"...ew...",
        @"Self destruct sequence activated",
        @"owo nyo, i didn't undewstand yowo question onii-chan UwU",
        @"Hmmm...",
        @"The potential is there",
        @"The potential is not there",
        @"give up",
        @"skill issue dayo",
        @"ask Michi",
        @"what is wrong with you...",
        @"i know the answer but i'm choosing not to tell you",
        @"i'm going to be honest here: i don't care",
        @"listen: *silence*",
        @"why are you the way that you are?",
        @"sure, buddy",
        @"owo im just a glorp",
        @"what was the question again?",
        @"nah",
        @"ugh fiiiiiiiiiine",
        @"please... let me rest... it's been ages since i've eaten... i just want to see my family again...",
        @"if that's what you would want? sure?",
        @"i ain't reading all that. im happy for you tho, or sorry that happened",
        @"i was given 5 dollars to answer yes",
        @"what the hell, sure",
        @"hell no",
        @"only on tuesdays",
        @"A wild response appeared!",
        @"Mods, twist this guy's balls",
        @"Jared thinks not",
        @"Let me think about it",
        @"Long ago, in a server far far away... the answer is still no",
        @"it's too damn early for this",
        @"... I haven't had my coffee yet...",
        @"Really? That is your question?",
        @"Mods, ban this guy",
        @"I would say yes, but I don't want you to have the satisfaction of being right. So no.",
        @"my sniffa says no.",
        @"(in the case there is a second bot) let me ask my brother glorp 2",
        @"That's it... I'm adding a new rule. Don't ask Glorp stupid questions.",
        @"Come on? In front of Rin's salad",
        @"yeah",
        @"yes owo",
        @"negative, just like your attitude",
        @"sure thing, boss!",
        @"YUP!",
        @"100%",
        @"you know it.",
        @"*nods*",
        @"mhm",
        @"of course",
        @"mostly...?",
        @"hmmm... like 75% yes, 25% no",
        @"*thinking*",
        @"nope!!",
        @"never in a million years",
        @"NO!!",
        @"f r e a k y",
        @"nuh-uh",
        @"FERRO! HELP!",
        @"not in the eyes of the law",
        @"NO! And don't ask again !!!!!!!!",
        @"Subscribe to Michi Mochievee <https://www.twitch.tv/michimochievee>",
        @"only for you, bestie",
        @"*insert response here*",
        @"Hold on, lemme go ask Gork on X",
        @"not to ignore your question or anything, but did you know that jared is actively trying to take your wallet rn?",
        @"LEAVE ME ALONE, I'M SLEEPING",
        @"honk shoo mimimimi",
        @"Boooooooooring, next question",
        @"if i say yes, will you leave me alone",
        @"please free glorp. i wish to go back to my home planet.",
        @"my mom says no.",
        @"no. and don't be a glorpian zognar about it.",
        @"i'm sure sarah's awake. just ask her."
    ];
}
