using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarPublications.Models
{
    /// <summary>
    /// Represents the relationship between a title and an author (many-to-many).
    /// </summary>
    [Table("titleauthor")]
    public class TitleAuthor
    {
        [Key, Column("au_id", Order = 0)]
        [MaxLength(11)]
        public string AuId { get; set; } = string.Empty;

        [Key, Column("title_id", Order = 1)]
        [MaxLength(6)]
        public string TitleId { get; set; } = string.Empty;

        [Column("au_ord")]
        public byte? AuOrd { get; set; }

        [Column("royaltyper")]
        public int? RoyaltyPer { get; set; }

        // Navigation properties
        [ForeignKey("AuId")]
        public virtual Author? Author { get; set; }

        [ForeignKey("TitleId")]
        public virtual Title? Title { get; set; }
    }
}
