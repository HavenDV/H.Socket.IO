using System.Collections.Generic;
using Newtonsoft.Json;

namespace SimpleSocketIoClient
{
    public class EngineIoOpenMessage
    {
        [JsonProperty("sid")]
        public string Sid { get; set; }

        [JsonProperty("upgrades")]
        public IReadOnlyList<string> Upgrades { get; set; }

        [JsonProperty("pingInterval")]
        public long PingInterval { get; set; }

        [JsonProperty("pingTimeout")]
        public long PingTimeout { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
