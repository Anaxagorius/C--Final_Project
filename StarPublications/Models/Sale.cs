using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarPublications.Models
{
    /// <summary>
    /// Represents a sales order in the PUBs database.
    /// The sales table uses a composite primary key: stor_id + ord_num + title_id.
    /// </summary>
    [Table("sales")]
    public class Sale
    {
        [Key, Column("stor_id", Order = 0)]
        [MaxLength(4)]
        public string StorId { get; set; } = string.Empty;

        [Key, Column("ord_num", Order = 1)]
        [MaxLength(20)]
        public string OrdNum { get; set; } = string.Empty;

        [Column("ord_date")]
        public DateTime OrdDate { get; set; }

        [Column("qty")]
        public short Qty { get; set; }

        [Column("payterms")]
        [MaxLength(12)]
        public string PayTerms { get; set; } = string.Empty;

        [Key, Column("title_id", Order = 2)]
        [MaxLength(6)]
        public string TitleId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("StorId")]
        public virtual Store? Store { get; set; }

        [ForeignKey("TitleId")]
        public virtual Title? Title { get; set; }
    }
}
