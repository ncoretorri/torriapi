using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Torri.Models;

public partial class SerieMask
{
    public int? FixSeason { get; set; }
    public string Mask { get; set; } = string.Empty;

    [JsonIgnore] public Regex Regex { get; private set; } = null!;

    public void GenerateRegex(ILogger logger)
    {
        if (Regex != null!)
            return;

        var eIndex = 0;
        var escaped = Regex.Escape(Mask).Replace("\\#", "#");

        var regex = MaskRegex().Replace(escaped, match =>
        {
            var reg = new StringBuilder(2 * match.Length).Insert(0, "\\d", match.Length);
            var name = match.Value.StartsWith('@') ? "s" : $"e{eIndex++}";
            return $"(?<{name}>{reg})";
        });
        
        logger.LogInformation("Generated regex: {Regex}", regex);

        Regex = new Regex(regex, RegexOptions.IgnoreCase);
    }

    [GeneratedRegex("(##?)|(@@?)|(&&?)")]
    private static partial Regex MaskRegex();
}