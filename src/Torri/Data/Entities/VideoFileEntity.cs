using System.ComponentModel.DataAnnotations;

namespace Torri.Data.Entities;

public class VideoFileEntity
{
    public int Id { get; set; }
    public int FileIndex { get; set; }
    public int TorrentEntityId { get; set; }
    [MaxLength(1000)] public string Path { get; set; } = string.Empty;
    public bool IsProcessed { get; set; }
    public bool Wanted { get; set; }

    [MaxLength(200)] public string EpisodeName { get; set; } = string.Empty;

    public int Season { get; set; }
    
    public TorrentEntity TorrentEntity { get; set; } = null!;
}