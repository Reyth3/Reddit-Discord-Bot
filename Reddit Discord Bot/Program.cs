using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Commands;

namespace DiscordBot
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
        }

        static void Main(string[] args) => new Program().Start().Wait();

        public async Task Start()
        {
            using (Client = new DiscordSocketClient())
            {
                Commands = new CommandService();

                Client.Ready += async () =>
                {
                    //#if RELEASE
                    //				foreach(var channel in Client.Guilds)
                    //				{
                    //					//annoying, might not do this
                    //					await channel.DefaultChannel.SendMessageAsync("QBOT INITIALIZATION DONE, FULLY OPERATIONAL!");
                    //				}
                    //#endif
                    Log("Bot", "Bot fully operational!");
                    await Task.CompletedTask;
                };

                Client.Disconnected += async c =>
                {
                    Console.WriteLine("Bot shutting down");
                    await Task.CompletedTask;
                };

                await Client.LoginAsync(TokenType.Bot, Q.Token);
                await Client.StartAsync();

                await InstallCommands();

                //Wait 1 second to see if the exit bool has changed
                while (!IsExit) await Task.Delay(1000);

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
            if (!message.HasCharPrefix('q', ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos) || message.Author.IsBot) return;

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