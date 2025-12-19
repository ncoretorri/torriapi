using System.ComponentModel.DataAnnotations;
using Torri.Data.Enums;

namespace Torri.Data.Entities;

public class TorrentEntity
{
    public int Id { get; set; }

    [MaxLength(30)] public string ExternalId { get; set; } = string.Empty;

    [MaxLength(100)] public string Hash { get; set; } = string.Empty;

    public bool IsProcessed { get; set; }

    public bool HasMissingRegex { get; set; }
    
    public bool IsRunning { get; set; }

    public TorrentType TorrentType { get; init; } = TorrentType.None;

    [MaxLength(250)] public string TorrentName { get; init; } = string.Empty;

    [MaxLength(250)] public string DisplayName { get; init; } = string.Empty;

    public ICollection<VideoFileEntity> VideoFiles { get; set; } = [];

    public ICollection<SerieMaskEntity> SerieMasks { get; set; } = [];
}