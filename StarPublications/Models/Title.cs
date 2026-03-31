using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarPublications.Models
{
    /// <summary>
    /// Represents a book title in the PUBs database.
    /// </summary>
    [Table("titles")]
    public class Title
    {
        [Key]
        [Column("title_id")]
        [MaxLength(6)]
        public string TitleId { get; set; } = string.Empty;

        [Column("title")]
        [MaxLength(80)]
        public string TitleName { get; set; } = string.Empty;

        [Column("type")]
        [MaxLength(12)]
        public string? Type { get; set; }

        [Column("pub_id")]
        [MaxLength(4)]
        public string? PubId { get; set; }

        [Column("price", TypeName = "money")]
        public decimal? Price { get; set; }

        [Column("advance", TypeName = "money")]
        public decimal? Advance { get; set; }

        [Column("royalty")]
        public int? Royalty { get; set; }

        [Column("ytd_sales")]
        public int? YtdSales { get; set; }

        [Column("notes")]
        [MaxLength(200)]
        public string? Notes { get; set; }

        [Column("pubdate")]
        public DateTime PubDate { get; set; }

        // Navigation properties
        [ForeignKey("PubId")]
        public virtual Publisher? Publisher { get; set; }

        public virtual ICollection<TitleAuthor> TitleAuthors { get; set; } = new List<TitleAuthor>();
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
