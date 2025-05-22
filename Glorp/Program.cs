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

            IDisposable? stopTyping = null;
            var replyAsync = arg.ReplyAsync;

            for (var count = 0; count < 3; count++)
            {
                stopTyping?.Dispose();
                stopTyping = null;

                // Have a chance to use kuku-only replies with kuku.
                var replyArray = arg.Author.Id == KukuId && Random.Shared.NextSingle() <= 0.25
                    ? s_kukuReplies
                    : s_replies;

                var reply = replyArray[Random.Shared.Next(0, replyArray.Length)];
                var msg = await replyAsync(new ReplyMessageProperties
                {
                    Content = reply.Text,
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
                replyAsync = msg.ReplyAsync;

                if (reply.Type != ReplyType.Stalling)
                    break; // Quit looping if reply is not a stalling one.

                stopTyping = arg.Channel?.EnterTypingStateAsync();

                // Delay between 1 and 3 seconds before replying.
                await Task.Delay(Random.Shared.Next(1000, 3000));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to reply to message {Id}.", arg.Id);
        }
    }

    private const ulong KukuId = 429297679445655572;

    private static readonly ImmutableArray<Reply> s_replies = [
        new Reply(
            "too busy thinkin about tiddies to answer",
            ReplyType.NonSequitur
        ),
        new Reply("you know what? hell yeah!!!", ReplyType.Yes),
        new Reply(
            "Saya tidak bisa memenuhi permintaan Anda karena alasan pribadi.",
            ReplyType.Yes
        ),
        new Reply("*thinking*", ReplyType.Stalling),
        new Reply(
            "I would say yes, but I don't want you to have the satisfaction of being right. So no.",
            ReplyType.Yes
        ),
        new Reply("...ew...", ReplyType.NonSequitur),
        new Reply("95% yes, 5% HUH", ReplyType.Yes),
        new Reply(
            "Holding your hand gently when I say this... absolutely the fuck not, brother. It was not even a possibility.",
            ReplyType.Yes
        ),
        new Reply("Perhaps", ReplyType.Maybe),
        new Reply("Okie! Sounds good!", ReplyType.Yes),
        new Reply("*sadly* Yeah...", ReplyType.Yes),
        new Reply("sure, buddy", ReplyType.Yes),
        new Reply("only on tuesdays", ReplyType.Maybe),
        new Reply("Holy smokes, you're kinda cookin here...", ReplyType.Yes),
        new Reply("owo im just a glorp", ReplyType.NonSequitur),
        new Reply("100%", ReplyType.Yes),
        new Reply("never in a million years", ReplyType.No),
        new Reply("Yes", ReplyType.Yes),
        new Reply(
            "Why does your mind conjure such questions?",
            ReplyType.NonSequitur
        ),
        new Reply("my sniffa says no.", ReplyType.No),
        new Reply("what was the question again?", ReplyType.NonSequitur),
        new Reply(
            "please free glorp. i wish to go back to my home planet.",
            ReplyType.NonSequitur
        ),
        new Reply("f r e a k y", ReplyType.NonSequitur),
        new Reply(
            "I'm frightened by your thought process.",
            ReplyType.NonSequitur
        ),
        new Reply("FERRO! HELP!", ReplyType.NonSequitur),
        new Reply("Yep!", ReplyType.Yes),
        new Reply("Baby, that's an inside thought.", ReplyType.No),
        new Reply("my mom says no.", ReplyType.No),
        new Reply("¯\\\\\\_(ツ)\\_/¯", ReplyType.NonSequitur),
        new Reply("yeah", ReplyType.Yes),
        new Reply(
            "no. and don't be a glorpian zognar about it.",
            ReplyType.No
        ),
        new Reply("honk shoo mimimimi", ReplyType.NonSequitur),
        new Reply(
            "i ain't reading all that. im happy for you tho, or sorry that happened",
            ReplyType.NonSequitur
        ),
        new Reply("NO!!", ReplyType.No),
        new Reply("A wild response appeared!", ReplyType.NonSequitur),
        new Reply("This pleases Glorp.", ReplyType.Yes),
        new Reply("mhm", ReplyType.Yes),
        new Reply("YOU AND ME FINALLY AGREE.", ReplyType.Yes),
        new Reply("only for you, bestie", ReplyType.Yes),
        new Reply("Saya tidak berminat sih.", ReplyType.NonSequitur),
        new Reply("Perchance", ReplyType.Maybe),
        new Reply("Input invalid", ReplyType.Maybe),
        new Reply(
            "Sometimes, I wonder if it even matters...",
            ReplyType.NonSequitur
        ),
        new Reply("NO! And don't ask again !!!!!!!!", ReplyType.No),
        new Reply("Nah!!!!!", ReplyType.No),
        new Reply("get smoked, bozo.", ReplyType.NonSequitur),
        new Reply("Why ask me?", ReplyType.NonSequitur),
        new Reply("listen: *silence*", ReplyType.NonSequitur),
        new Reply("what the hell, sure", ReplyType.Yes),
        new Reply("Have you considered NOT doing that?", ReplyType.No),
        new Reply("Boooooooooring, next question", ReplyType.NonSequitur),
        new Reply("yes owo", ReplyType.Yes),
        new Reply(
            "You're basically asking mom if we can stop at McDonald's with this one.",
            ReplyType.NonSequitur
        ),
        new Reply("What do I look like, Google?", ReplyType.NonSequitur),
        new Reply(
            "How many times do I have to tell you, old man? NO!",
            ReplyType.No
        ),
        new Reply("i miss michi", ReplyType.NonSequitur),
        new Reply(
            "That's it... I'm adding a new rule. Don't ask Glorp stupid questions.",
            ReplyType.NonSequitur
        ),
        new Reply("Jared thinks not", ReplyType.No),
        new Reply("hell no", ReplyType.No),
        new Reply("Hold on, lemme go ask Gork on X...", ReplyType.Stalling),
        new Reply("I think it would be cool.", ReplyType.Yes),
        new Reply(
            "WHY WOULD YOU EVEN ASK THAT?! I'M REPORTING YOU TO THE GLORPTHORITIES.",
            ReplyType.NonSequitur
        ),
        new Reply("Mods, twist this guy's balls", ReplyType.NonSequitur),
        new Reply("The potential is there", ReplyType.Maybe),
        new Reply("nah", ReplyType.No),
        new Reply("Maybe", ReplyType.Maybe),
        new Reply("Self destruct sequence activated", ReplyType.NonSequitur),
        new Reply(
            "I've already answered this. Probably. IDK, ask something else.",
            ReplyType.NonSequitur
        ),
        new Reply("Who knows", ReplyType.NonSequitur),
        new Reply(
            "i know the answer but i'm choosing not to tell you",
            ReplyType.NonSequitur
        ),
        new Reply("Come on? In front of Rin's salad", ReplyType.NonSequitur),
        new Reply("FINALLY, SOMETHING I AGREE WITH", ReplyType.Yes),
        new Reply("No.", ReplyType.No),
        new Reply(
            "This has the same likelihood of the mods being freed... hehe, they're never getting out",
            ReplyType.No
        ),
        new Reply("nope!!", ReplyType.No),
        new Reply("Hmmm...", ReplyType.Stalling),
        new Reply("HELL YEAH !!!!! *high fives you*", ReplyType.Yes),
        new Reply("Really? That is your question?", ReplyType.NonSequitur),
        new Reply("*insert response here*", ReplyType.NonSequitur),
        new Reply(
            "i'm going to be honest here: i don't care",
            ReplyType.NonSequitur
        ),
        new Reply("*glorpily nods*", ReplyType.Yes),
        new Reply("yeah", ReplyType.Yes),
        new Reply("Betul sekali!!", ReplyType.Yes),
        new Reply("Nope.", ReplyType.No),
        new Reply(
            "I wanna agree, but I don't quite trust you.",
            ReplyType.Yes
        ),
        new Reply("ugh fiiiiiiiiiine", ReplyType.Yes),
        new Reply(
            "not to ignore your question or anything, but did you know that jared is actively trying to take your wallet rn?",
            ReplyType.NonSequitur
        ),
        new Reply("That sure is... a question", ReplyType.NonSequitur),
        new Reply("why are you the way that you are?", ReplyType.NonSequitur),
        new Reply("Ya, benar!!", ReplyType.Yes),
        new Reply("not in the eyes of the law", ReplyType.No),
        new Reply("You are a fool for even thinking about it.", ReplyType.No),
        new Reply("nooooooooooo", ReplyType.No),
        new Reply("Bro what?", ReplyType.NonSequitur),
        new Reply(
            "Subscribe to Michi Mochievee <https://www.twitch.tv/michimochievee>",
            ReplyType.NonSequitur
        ),
        new Reply("give up", ReplyType.No),
        new Reply("Wait a minute... who are you?", ReplyType.NonSequitur),
        new Reply("Let me think about it...", ReplyType.Stalling),
        new Reply("if i say yes, will you leave me alone", ReplyType.Yes),
        new Reply("*nods*", ReplyType.Yes),
        new Reply("Sounds good to me.", ReplyType.Yes),
        new Reply("Idfk", ReplyType.NonSequitur),
        new Reply("of course", ReplyType.Yes),
        new Reply(
            "please... let me rest... it's been ages since i've eaten... i just want to see my family again...",
            ReplyType.NonSequitur
        ),
        new Reply("Sure, buddy...", ReplyType.Yes),
        new Reply("i was given 5 dollars to answer yes", ReplyType.Yes),
        new Reply("Fat chance.", ReplyType.No),
        new Reply("Yes, but, I have some questions.", ReplyType.Yes),
        new Reply("Idk", ReplyType.NonSequitur),
        new Reply("negative, just like your attitude", ReplyType.No),
        new Reply(
            "owo nyo, i didn't undewstand yowo question onii-chan UwU",
            ReplyType.NonSequitur
        ),
        new Reply("N O !!!!!!!!!", ReplyType.No),
        new Reply("mostly...?", ReplyType.Maybe),
        new Reply("If i agree, will you give me some food?", ReplyType.Yes),
        new Reply("what is wrong with you...", ReplyType.NonSequitur),
        new Reply("Nuh-uh. Nope. Never.", ReplyType.No),
        new Reply(
            "NO!!! RIP BOZO!! EVERYONE GET A LOAD OF THIS GUY!! LMAOOOOOO",
            ReplyType.No
        ),
        new Reply("you know it.", ReplyType.Yes),
        new Reply("ERROR 404: SOLUTION NOT FOUND", ReplyType.NonSequitur),
        new Reply("if that's what you would want? sure?", ReplyType.Yes),
        new Reply("Never.", ReplyType.No),
        new Reply(
            "I was going to say no, but then I saw that cute little sparkle in your eye. Man, I can't say no to you.",
            ReplyType.Yes
        ),
        new Reply(
            "Don't think about whether or not it could happen. Think about if you should.",
            ReplyType.NonSequitur
        ),
        new Reply("No", ReplyType.No),
        new Reply(
            "If only you were asking to marry me :(",
            ReplyType.NonSequitur
        ),
        new Reply("*shakes head disapprovingly*", ReplyType.No),
        new Reply("I'm telling Ferro :/ ", ReplyType.NonSequitur),
        new Reply("Try again later", ReplyType.NonSequitur),
        new Reply(
            "Hold on, I'm still only 5 hours into the Michi vod. Can you check back in a few hours?",
            ReplyType.NonSequitur
        ),
        new Reply("it's too damn early for this", ReplyType.NonSequitur),
        new Reply("Oh Mama Mia!", ReplyType.NonSequitur),
        new Reply("Sure o3o", ReplyType.Yes),
        new Reply("wut", ReplyType.NonSequitur),
        new Reply("only if you ask nicely.", ReplyType.NonSequitur),
        new Reply("I'm down.", ReplyType.Yes),
        new Reply("Nah.", ReplyType.No),
        new Reply("YUP!", ReplyType.Yes),
        new Reply("*no, but in glorp*", ReplyType.No),
        new Reply(
            "Long ago, in a server far far away... the answer is still no",
            ReplyType.No
        ),
        new Reply(
            "Do you think I will see my mom again?",
            ReplyType.NonSequitur
        ),
        new Reply("sure thing, boss!", ReplyType.No),
        new Reply("hmmm... like 75% yes, 25% no", ReplyType.Yes),
        new Reply(
            "i'm sure sarah's awake. just ask her.",
            ReplyType.NonSequitur
        ),
        new Reply("Yeah!!!", ReplyType.Yes),
        new Reply("Saya tidak tahu ( •̯́ ₃ •̯̀)", ReplyType.Maybe),
        new Reply("skill issue dayo", ReplyType.NonSequitur),
        new Reply("Yeah, I think I'd like that.", ReplyType.Yes),
        new Reply(
            "On my home planet, yes. On this one? Absolutely not.",
            ReplyType.No
        ),
        new Reply(
            "Have you tried looking within yourself for the answer?",
            ReplyType.NonSequitur
        ),
        new Reply("I'm not authorized to answer that.", ReplyType.NonSequitur),
        new Reply("The potential is not there", ReplyType.No),
        new Reply("._.", ReplyType.NonSequitur),
        new Reply("Sure.", ReplyType.Yes),
        new Reply("nuh-uh", ReplyType.No),
        new Reply("Mods, ban this guy", ReplyType.NonSequitur),
        new Reply("... I haven't had my coffee yet...", ReplyType.NonSequitur),
        new Reply(
            "Sorry, I'm not taking questions right now. I'm busy studying the blade.",
            ReplyType.NonSequitur
        ),
        new Reply("I would like to think so.", ReplyType.Yes),
        new Reply(
            "Sometimes I open a fortune cookie and it has no fortune. It just tells me to have a good day and gives me some numbers. I'm not telling you to have a good day... but your lucky numbers are 12, 5, 23, 16, 14, and 33",
            ReplyType.NonSequitur
        ),
        new Reply("ask Michi", ReplyType.NonSequitur),
        new Reply("Y E S !!!!!!!!!!!!!!!", ReplyType.Yes),
        new Reply(
            "let me ask my brother glorp 2",
            ReplyType.NonSequitur
        ),
        new Reply("LEAVE ME ALONE, I'M SLEEPING", ReplyType.NonSequitur),
        new Reply("NO!! BAD DOG!!", ReplyType.No),
    ];

    private static readonly ImmutableArray<Reply> s_kukuReplies = [
        new Reply(
            "Idk kuku, have you tried thinking for yourself?",
            ReplyType.NonSequitur
        ),
        new Reply(
            "Kuku, this is the 10th time you have asked me something today",
            ReplyType.NonSequitur
        ),
        new Reply("Let me think about it poopooagiri...", ReplyType.Stalling),
        new Reply("Sorry, I don't speak to you kuku.", ReplyType.NonSequitur),
        new Reply(
            "Special answer only for you kuku: HELL NO, EVEN IF HELL FREEZES OVER.",
            ReplyType.No
        ),
        new Reply(
            "Go crazy girl, gotta train that shit to failure",
            ReplyType.Yes
        ),
    ];
}

internal readonly record struct Reply(string Text, ReplyType Type);

internal enum ReplyType
{
    Yes,
    No,
    Maybe,
    NonSequitur,
    Stalling,
}
