using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TurtleBot.Services;

namespace TurtleBot.Modules
{
    public class RainModule : ModuleBase<SocketCommandContext>
    {
        private readonly RainService _rainService;
        private readonly WalletService _walletService;
        private readonly IConfiguration _config;

        public RainModule(RainService rainService, WalletService walletService, IConfiguration config)
        {
            _rainService = rainService;
            _walletService = walletService;
            _config = config;
        }

        [Command("rain")]
        [Summary("Checks the weather")]
        public async Task Rain()
        {
            try {
                switch (_rainService.State)
                {
                    case RainServiceState.CheckingBalance:
                        long balance = await _walletService.GetBalance(_rainService.BotWallet);
                        decimal missing = (_rainService.BalanceThreshold - balance) / 100.0M;
                        var embed = new EmbedBuilder()
                            .WithColor(new Color(114, 137, 218))
                            .WithTitle($"Looks like the rain is still ```{missing}``` TRTL away...")
                            .WithDescription($"Donate TRTL to make it rain again! ```\n{_rainService.BotWallet.Address}```")
                            .WithThumbnailUrl(_config["rainImageUrlTRTL"])
                            .Build();
                        await ReplyAsync("", false, embed);
                        return;

                    case RainServiceState.BalanceExceeded:
                    case RainServiceState.AcceptingRegistrations:
                    case RainServiceState.Raining:
                        await ReplyAsync($"Be patient, little turtle, it's raining soon.");
                        return;

                    case RainServiceState.Stopped:
                    default:
                        await ReplyAsync($"The sky is blue, no rain for today...");
                        return;
                }
            } catch (Exception) {
                await ReplyAsync($"The sky is blue, no rain for today...");
            }
        }

        [Command("rain")]
        [Summary("Controls the rain function")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Rain(string subCommand)
        {
            switch (subCommand.ToLowerInvariant())
            {
                case "start":
                    _rainService.Start();
                    await ReplyAsync("Better watch out for rain, my little turtles...");
                    return;
                case "stop":
                    _rainService.Stop();
                    await ReplyAsync("Okay, time to go home turtles, it's not raining today.");
                    return;
            }
        }
    }
}