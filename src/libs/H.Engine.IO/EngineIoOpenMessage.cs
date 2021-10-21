using Newtonsoft.Json;

namespace H.Engine.IO;

/// <summary>
/// 
/// </summary>
public class EngineIoOpenMessage
{
    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("sid")]
    public string? Sid { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("upgrades")]
    public IReadOnlyList<string>? Upgrades { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("pingInterval")]
    public long PingInterval { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("pingTimeout")]
    public long PingTimeout { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
