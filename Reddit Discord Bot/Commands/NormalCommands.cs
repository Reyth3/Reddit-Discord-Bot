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

namespace Reddit_Discord_Bot.Commands
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

        [Command("setchannel")]
        [Summary("Sets the current channel as the target channel for new Reddit posts")]
        [Alias("setch", "channel", "ch")]
        public async Task SetChannel()
        {
            var sId = Context.Guild.Id;
            var server = ServerConfiguration.GlobalConfig.FirstOrDefault(o => o.ServerId == sId);
            if (server == null)
            {
                ServerConfiguration.GlobalConfig.Add(new ServerConfiguration());
                server = ServerConfiguration.GlobalConfig.Last();
            }
            server.ServerId = sId;
            server.ChannelId = Context.Channel.Id;
            ServerConfiguration.SaveConfig();
            await ReplyAsync("Reddit Feed Channel set to: ***" + Context.Channel.Name + "***");
        }

        [Command("setsubs")]
        [Summary("Set subreddits for this server.")]
        [Alias("subreddits", "subs")]
        public async Task Square([Summary("Subreddits' names without the '/r/' part, separated with | sign")] string subs)
        {
            var subredditNames = subs.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(o => o.Replace("/r/", "").Replace("r/", "").Trim());
            var sId = Context.Guild.Id;
            var server = ServerConfiguration.GlobalConfig.FirstOrDefault(o => o.ServerId == sId);
            if (server == null)
            {
                ServerConfiguration.GlobalConfig.Add(new ServerConfiguration());
                server = ServerConfiguration.GlobalConfig.Last();
            }
            server.ServerId = Context.Guild.Id;
            server.Subreddits = subredditNames.ToArray();
            ServerConfiguration.SaveConfig();
            await ReplyAsync("Successfully set " + subredditNames.Count() + " subreddits.");
        }

        [Command("cleanup")]
        [Summary("Cleans up the mess this bot makes.")]
        [Alias("clean", "clear", "cls", "bsod")]
        public async Task Clean(IUser user)
        {
            var messages = await Context.Channel.GetMessagesAsync(100).Flatten();
            await Context.Channel.DeleteMessagesAsync(messages.Where(o => o.Author.Id == user.Id));
        }

        [Command("enable")]
        [Summary("Enables or disables the bot on this server. rb!enable <true|false>")]
        [Alias("on", "works", "up")]
        public async Task Enable(bool enable)
        {
            var sId = Context.Guild.Id;
            var serv = ServerConfiguration.GlobalConfig.FirstOrDefault(o => o.ServerId == sId);
            if (serv != null)
                serv.Enabled = enable;
            ServerConfiguration.SaveConfig();
            await ReplyAsync("Reddit Bot has been ***" + (enable ? "enabled" : "disabled") + "*** on this server.");
        }
    }
}