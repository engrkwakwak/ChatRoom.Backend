using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatRoom.UnitTest.Helpers
{
    public class ConfigurationFactory
    {
        public static IConfiguration GenerateJwtSettings()
        {
            Dictionary<string, object> settings = new Dictionary<string, object>();
            settings.Add("validIssuer", "https://localhost:5001");
            settings.Add("validAudience", "https://localhost:5001");
            settings.Add("expires", 60);

            IEnumerable<KeyValuePair<string, string?>> inMemorySettings = new Dictionary<string, string?> {
                { "JwtSettings", JsonSerializer.Serialize(settings) }
            };

            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
            return config;
        }

        public static IConfiguration GenerateEmptyConfiguration() 
        {
            IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection().Build();
            return config;
        }

        public static string GetTokenSecretKey() 
        {
            return "cHaTrOoM-sEcReTkEy-1$2$3$4$5$6$7$8$9$0$-GREATERTHAN256BYTES";
        }
    }
}
