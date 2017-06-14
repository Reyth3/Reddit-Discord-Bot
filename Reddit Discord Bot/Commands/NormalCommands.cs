using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DiscordBot.Commands
{
    public class NormalCommands : ModuleBase
    {
        [Command("help")]
        [Summary("I will tell you about all of my commands.")]
        public async Task Help()
        {
            EmbedBuilder eb = new EmbedBuilder();

            MethodInfo[] mi = GetType().GetMethods();
            for (int o = 0; o < mi.Length; o++)
            {
                CommandAttribute myAttribute1 = mi[o].GetCustomAttributes(true).OfType<CommandAttribute>().FirstOrDefault();
                SummaryAttribute myAttribute2 = mi[o].GetCustomAttributes(true).OfType<SummaryAttribute>().FirstOrDefault();
                if (myAttribute1 != null && myAttribute2 != null)
                    eb.AddField(myAttribute1.Text, myAttribute2.Text);
            }

            await ReplyAsync("", false, eb);
        }

        [Command("say")]
        [Summary("says whatever you say")]
        [Alias("echo")]
        public async Task Say([Remainder] string echo)
        {
            //await Context.Message.DeleteAsync();
            await ReplyAsync(echo);
        }

        // ~sample square 20 -> 400
        [Command("square")]
        [Summary("Squares a number.")]
        public async Task Square([Summary("The number to square.")] int num)
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }
    }
}