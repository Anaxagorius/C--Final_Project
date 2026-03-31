using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarPublications.Models
{
    /// <summary>
    /// Represents publisher information (logo and PR blurb) in the PUBs database.
    /// </summary>
    [Table("pub_info")]
    public class PubInfo
    {
        [Key]
        [Column("pub_id")]
        [MaxLength(4)]
        public string PubId { get; set; } = string.Empty;

        [Column("pr_info")]
        public string? PrInfo { get; set; }

        // Navigation property
        [ForeignKey("PubId")]
        public virtual Publisher? Publisher { get; set; }
    }
}
