using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarPublications.Models
{
    /// <summary>
    /// Represents a publisher in the PUBs database.
    /// </summary>
    [Table("publishers")]
    public class Publisher
    {
        [Key]
        [Column("pub_id")]
        [MaxLength(4)]
        public string PubId { get; set; } = string.Empty;

        [Column("pub_name")]
        [MaxLength(40)]
        public string? PubName { get; set; }

        [Column("city")]
        [MaxLength(20)]
        public string? City { get; set; }

        [Column("state")]
        [MaxLength(2)]
        public string? State { get; set; }

        [Column("country")]
        [MaxLength(30)]
        public string? Country { get; set; }

        // Navigation property
        public virtual ICollection<Title> Titles { get; set; } = new List<Title>();
        public virtual ICollection<PubInfo> PubInfos { get; set; } = new List<PubInfo>();
    }
}
