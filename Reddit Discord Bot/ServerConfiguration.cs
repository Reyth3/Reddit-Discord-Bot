using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reddit_Discord_Bot
{
    public class ServerConfiguration
    {
        public static List<ServerConfiguration> GlobalConfig { get; set; }

        public static void LoadConfig()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "servers.config");
            if (File.Exists(path))
                GlobalConfig = JsonConvert.DeserializeObject<List<ServerConfiguration>>(File.ReadAllText(path));
            else GlobalConfig = new List<ServerConfiguration>();
        }

        public static void SaveConfig()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "servers.config");
            File.WriteAllText(path, JsonConvert.SerializeObject(GlobalConfig, Formatting.Indented));
        }

        public bool Enabled { get; set; }
        public ulong ServerId { get; set; }
        public ulong ChannelId { get; set; }
        public string[] Subreddits { get; set; }
    }
}
