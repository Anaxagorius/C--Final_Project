using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarPublications.Models
{
    /// <summary>
    /// Represents a store location in the PUBs database.
    /// </summary>
    [Table("stores")]
    public class Store
    {
        [Key]
        [Column("stor_id")]
        [MaxLength(4)]
        public string StorId { get; set; } = string.Empty;

        [Column("stor_name")]
        [MaxLength(40)]
        public string? StorName { get; set; }

        [Column("stor_address")]
        [MaxLength(40)]
        public string? StorAddress { get; set; }

        [Column("city")]
        [MaxLength(20)]
        public string? City { get; set; }

        [Column("state")]
        [MaxLength(2)]
        public string? State { get; set; }

        [Column("zip")]
        [MaxLength(5)]
        public string? Zip { get; set; }

        // Navigation property
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
