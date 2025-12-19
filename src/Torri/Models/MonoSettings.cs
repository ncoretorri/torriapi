using MonoTorrent.Client;
using Riok.Mapperly.Abstractions;

namespace Torri.Models;

public class MonoSettings
{
    public int MaximumConnections { get; set; }
    public int MaximumDownloadRate { get; set; }
    public int DiskCacheBytes { get; set; }
    public int MaximumDiskReadRate { get; set; }
    public int MaximumDiskWriteRate { get; set; }
    public int MaximumHalfOpenConnections { get; set; }
    public int MaximumOpenFiles { get; set; }
    public int MaximumUploadRate { get; set; }
}

[Mapper]
public static partial class MonoSettingsMapper
{
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial MonoSettings ToUserSettings(EngineSettings settings);
    
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial void MapUserSettings(MonoSettings source, EngineSettingsBuilder target);
}