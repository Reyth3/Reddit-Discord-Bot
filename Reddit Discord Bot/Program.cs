using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Reddit_Discord_Bot.Commands;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace Reddit_Discord_Bot
{
    class Program
    {
        public static DiscordSocketClient Client;

        public static bool IsExit = false;

        CommandService Commands;
        public static Random R { get; } = new Random();

        public static void Log(string module, string message, LogType logType = LogType.Success)
        {
            var now = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[{0} {1}] ({2}) ", now.ToShortDateString(), now.ToShortTimeString(), module);
            Console.ForegroundColor = logType == LogType.Success ? ConsoleColor.Green : logType == LogType.Warning ? ConsoleColor.DarkYellow : ConsoleColor.Red;
            Console.Write("{0}", message);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\r\n");
        }

        static void Main(string[] args) => new Program().Start().Wait();

        static void AddBot()
        {
            Console.WriteLine("Do you want to add the bot to a server? (y/n)");
            var res = Console.ReadKey();
            if (res.Key == ConsoleKey.Y)
                Process.Start("https://discordapp.com/api/oauth2/authorize?client_id="+BotToken.Id+ "&scope=bot&permissions=224320");
            Console.Clear();
        }

        static Dictionary<string, List<Data1>> previousPosts { get; set; }
        static async Task GetRedditFeed()
        {
            var now = DateTime.Now;
            var subs = ServerConfiguration.GlobalConfig.Where(o => o.Subreddits != null).SelectMany(o => o.Subreddits).Distinct().ToArray();
            var dict = new Dictionary<string, List<Data1>>();
            using (WebClient wc = new WebClient())
            {
                foreach (var s in subs)
                {
                    var json = await wc.DownloadStringTaskAsync("https://www.reddit.com/r/" + s + "/new/.json?limit=5&rand=");
                    var result = JsonConvert.DeserializeObject<Rootobject>(json);
                    if (result.data.children != null)
                        dict.Add(s, result.data.children.Select(o => o.data).ToList());
                    await Task.Delay(200);
                }
            }
            foreach (var serv in ServerConfiguration.GlobalConfig)
            {
                if (serv.ServerId == 0 || serv.ChannelId == 0 || serv.Subreddits == null || serv.Enabled == false)
                    continue;
                foreach(var sub in serv.Subreddits)
                    if (dict.ContainsKey(sub))
                    {
                        if ((previousPosts != null && previousPosts.ContainsKey(sub)))
                        {
                            var newPosts = dict[sub].Where(o => !previousPosts[sub].Contains(o)).OrderByDescending(o => o.id);
                            var channel = Client.Guilds.First(o => o.Id == serv.ServerId).GetTextChannel(serv.ChannelId);
                            foreach (var p in newPosts)
                            {
                                var text = "***Title:*** *" + p.title + "* | ***/r/" + p.subreddit + "***\r\n" + p.url;
                                await channel.SendMessageAsync(text, false);
                                await Task.Delay(500);
                            }
                        }
                    }
            }
            if (previousPosts == null)
                previousPosts = new Dictionary<string, List<Data1>>();
            foreach (var sub in dict.Keys)
                if (previousPosts.ContainsKey(sub))
                    foreach (var post in dict[sub])
                    {
                        if (!previousPosts[sub].Contains(post))

                            previousPosts[sub].Add(post);
                    }
                else previousPosts.Add(sub, dict[sub]);
            Log("Bot", "Finished checking for new posts and sending updates. (" + (DateTime.Now - now).TotalSeconds.ToString("0.00") + "s)", LogType.Success);
        }

        public async Task Start()
        {
            AddBot();
            ServerConfiguration.LoadConfig();
            using (Client = new DiscordSocketClient())
            {
                Commands = new CommandService();

                Client.Ready += async () =>
                {
                    Log("Bot", "Bot fully operational!");
                    var notConfiguredServers = Client.Guilds.Where(o => ServerConfiguration.GlobalConfig.FirstOrDefault(p => p.ServerId == o.Id) == null);
                    if(notConfiguredServers.Count() != 0)
                    {
                        var names = "";
                        foreach (var s in notConfiguredServers) names += "\t=> " + s.Name + "\r\n";
                        Log("Bot", "The bot is not configured on following servers: \r\n" + names + "\r\nUse the following commands on these servers:\r\n\trb!setchannel -- on a channel you want to send the posts to;\r\n\trb!setsubs subreddit1|subreddit2|...", LogType.Warning);
                    }
                    while (true) await GetRedditFeed();
                    await Task.CompletedTask;
                };

                Client.Disconnected += async c =>
                {
                    Console.WriteLine("Bot shutting down");
                    await Task.CompletedTask;
                };

                await Client.LoginAsync(TokenType.Bot, BotToken.Token);
                await Client.StartAsync();

                await InstallCommands();

                //Wait 1 second to see if the exit bool has changed
                while (!IsExit) await Task.Delay(100);

                //Never do this.....
                //await Task.Delay(-1);

                await Client.LogoutAsync();
            }
        }

        async Task InstallCommands()
        {
            Client.MessageReceived += MessageReceived;

            await Commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        async Task MessageReceived(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            if (!message.HasStringPrefix("rb!", ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos) || message.Author.IsBot) return;

            var context = new CommandContext(Client, message);

            var result = await Commands.ExecuteAsync(context, argPos);

            //#if DEBUG
            //			if(!result.IsSuccess)
            //			{
            //				await context.Channel.SendMessageAsync(result.ErrorReason);
            //			}
            //#endif
        }
    }

    public enum LogType { Success, Warning, Error }
}