using System.Globalization;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.Commands;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDiscordGateway(static opts =>
{
    opts.Intents = GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent;
}).AddCommands(static opts =>
{
    opts.CultureInfo = CultureInfo.GetCultureInfo("en-US");
    opts.UseScopes   = true;
});

var host = builder.Build();

await host.RunAsync();

[GatewayEvent(nameof(GatewayClient.MessageCreate)), UsedImplicitly]
public sealed class MessageCreateHandler(ILogger<MessageCreateHandler> logger) : IGatewayEventHandler<Message>
{
    /// <inheritdoc />
    public ValueTask HandleAsync(Message arg)
    {
        logger.LogInformation(
            "[message] {Guild} #{Channel}: {Message}",
            arg.Guild?.Name ?? arg.Author.GlobalName,
            arg.Channel?.Id,
            arg.Content);
        return ValueTask.CompletedTask;
    }
}
