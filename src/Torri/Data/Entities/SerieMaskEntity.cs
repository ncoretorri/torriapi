using System.ComponentModel.DataAnnotations;

namespace Torri.Data.Entities;

public class SerieMaskEntity
{
    public int Id { get; set; }
    public int TorrentEntityId { get; set; }
    public int? FixSeason { get; set; }
    [MaxLength(50)] public string Mask { get; set; } = string.Empty;

    public TorrentEntity TorrentEntity { get; set; } = null!;
}