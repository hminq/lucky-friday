using System.ComponentModel.DataAnnotations;

namespace LuckyFridayCalculator.Models;

public class Member
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    public virtual ICollection<LineupEntry> LineupEntries { get; set; } = new List<LineupEntry>();
}