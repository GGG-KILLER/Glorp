using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables("GLORP_");

builder.Services.AddSystemd()
    .AddDiscordGateway(static opts =>
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
        @"too busy thinkin about tiddies to answer",
        @"you know what? hell yeah!!!",
        @"Saya tidak bisa memenuhi permintaan Anda karena alasan pribadi.",
        @"*thinking*",
        @"I would say yes, but I don't want you to have the satisfaction of being right. So no.",
        @"...ew...",
        @"95% yes, 5% HUH",
        @"Holding your hand gently when I say this... absolutely the fuck not, brother. It was not even a possibility.",
        @"Perhaps",
        @"Okie! Sounds good!",
        @"*sadly* Yeah...",
        @"sure, buddy",
        @"only on tuesdays",
        @"Holy smokes, you're kinda cookin here...",
        @"owo im just a glorp",
        @"100%",
        @"never in a million years",
        @"Yes",
        @"Why does your mind conjure such questions?",
        @"my sniffa says no.",
        @"what was the question again?",
        @"please free glorp. i wish to go back to my home planet.",
        @"f r e a k y",
        @"I'm frightened by your thought process.",
        @"FERRO! HELP!",
        @"Yep!",
        @"Baby, that's an inside thought.",
        @"my mom says no.",
        @"¯\\\_(ツ)\_/¯",
        @"yeah",
        @"no. and don't be a glorpian zognar about it.",
        @"honk shoo mimimimi",
        @"i ain't reading all that. im happy for you tho, or sorry that happened",
        @"NO!!",
        @"A wild response appeared!",
        @"This pleases Glorp.",
        @"mhm",
        @"YOU AND ME FINALLY AGREE.",
        @"only for you, bestie",
        @"Saya tidak berminat sih.",
        @"Perchance",
        @"Input invalid",
        @"Sometimes, I wonder if it even matters...",
        @"NO! And don't ask again !!!!!!!!",
        @"Nah!!!!!",
        @"get smoked, bozo.",
        @"Why ask me?",
        @"listen: *silence*",
        @"what the hell, sure",
        @"Have you considered NOT doing that?",
        @"Boooooooooring, next question",
        @"yes owo",
        @"You're basically asking mom if we can stop at McDonald's with this one.",
        @"What do I look like, Google?",
        @"How many times do I have to tell you, old man? NO!",
        @"i miss michi",
        @"That's it... I'm adding a new rule. Don't ask Glorp stupid questions.",
        @"Jared thinks not",
        @"hell no",
        @"Hold on, lemme go ask Gork on X",
        @"I think it would be cool.",
        @"WHY WOULD YOU EVEN ASK THAT?! I'M REPORTING YOU TO THE GLORPTHORITIES.",
        @"Mods, twist this guy's balls",
        @"The potential is there",
        @"nah",
        @"Maybe",
        @"Self destruct sequence activated",
        @"I've already answered this. Probably. IDK, ask something else.",
        @"Who knows",
        @"i know the answer but i'm choosing not to tell you",
        @"Come on? In front of Rin's salad",
        @"FINALLY, SOMETHING I AGREE WITH",
        @"No.",
        @"This has the same likelihood of the mods being freed... hehe, they're never getting out",
        @"nope!!",
        @"Hmmm...",
        @"HELL YEAH !!!!! *high fives you*",
        @"Really? That is your question?",
        @"*insert response here*",
        @"i'm going to be honest here: i don't care",
        @"*glorpily nods*",
        @"yeah",
        @"Betul sekali!!",
        @"Nope.",
        @"I wanna agree, but I don't quite trust you.",
        @"ugh fiiiiiiiiiine",
        @"not to ignore your question or anything, but did you know that jared is actively trying to take your wallet rn?",
        @"That sure is... a question",
        @"why are you the way that you are?",
        @"Ya, benar!!",
        @"not in the eyes of the law",
        @"You are a fool for even thinking about it.",
        @"nooooooooooo",
        @"Bro what?",
        @"Subscribe to Michi Mochievee <https://www.twitch.tv/michimochievee>",
        @"give up",
        @"Wait a minute... who are you?",
        @"Let me think about it",
        @"if i say yes, will you leave me alone",
        @"*nods*",
        @"Sounds good to me.",
        @"Idfk",
        @"of course",
        @"please... let me rest... it's been ages since i've eaten... i just want to see my family again...",
        @"Sure, buddy...",
        @"i was given 5 dollars to answer yes",
        @"Fat chance.",
        @"Yes, but, I have some questions.",
        @"Idk",
        @"negative, just like your attitude",
        @"owo nyo, i didn't undewstand yowo question onii-chan UwU",
        @"N O !!!!!!!!!",
        @"mostly...?",
        @"If i agree, will you give me some food?",
        @"what is wrong with you...",
        @"Nuh-uh. Nope. Never.",
        @"NO!!! RIP BOZO!! EVERYONE GET A LOAD OF THIS GUY!! LMAOOOOOO",
        @"you know it.",
        @"ERROR 404: SOLUTION NOT FOUND",
        @"if that's what you would want? sure?",
        @"Never.",
        @"I was going to say no, but then I saw that cute little sparkle in your eye. Man, I can't say no to you.",
        @"Don't think about whether or not it could happen. Think about if you should.",
        @"No",
        @"If only you were asking to marry me :(",
        @"*shakes head disapprovingly*",
        @"I'm telling Ferro :/ ",
        @"Try again later",
        @"Hold on, I'm still only 5 hours into the Michi vod. Can you check back in a few hours?",
        @"it's too damn early for this",
        @"Oh Mama Mia! ",
        @"Sure o3o",
        @"wut",
        @"only if you ask nicely.",
        @"I'm down.",
        @"Nah.",
        @"YUP!",
        @"*no, but in glorp*",
        @"Long ago, in a server far far away... the answer is still no",
        @"Do you think I will see my mom again?",
        @"sure thing, boss!",
        @"hmmm... like 75% yes, 25% no",
        @"i'm sure sarah's awake. just ask her.",
        @"Yeah!!!",
        @"Saya tidak tahu ( •̯́ ₃ •̯̀)",
        @"skill issue dayo",
        @"Yeah, I think I'd like that.",
        @"On my home planet, yes. On this one? Absolutely not.",
        @"Have you tried looking within yourself for the answer?",
        @"I'm not authorized to answer that.",
        @"The potential is not there",
        @"._.",
        @"Sure.",
        @"nuh-uh",
        @"Mods, ban this guy",
        @"... I haven't had my coffee yet...",
        @"Sorry, I'm not taking questions right now. I'm busy studying the blade.",
        @"I would like to think so.",
        @"Sometimes I open a fortune cookie and it has no fortune. It just tells me to have a good day and gives me some numbers. I'm not telling you to have a good day... but your lucky numbers are 12, 5, 23, 16, 14, and 33",
        @"ask Michi",
        @"Y E S !!!!!!!!!!!!!!!",
        @"(in the case there is a second bot) let me ask my brother glorp 2",
        @"LEAVE ME ALONE, I'M SLEEPING",
        @"NO!! BAD DOG!!",
    ];
}
