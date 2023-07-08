namespace H.Engine.IO;

/// <summary>
/// 
/// </summary>
public class EngineIoOpenMessage
{
    /// <summary>
    /// 
    /// </summary>
    public string? Sid { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public IReadOnlyList<string>? Upgrades { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public long PingInterval { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public long PingTimeout { get; set; }
}
