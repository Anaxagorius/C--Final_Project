using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarPublications.Models
{
    /// <summary>
    /// Represents an author in the PUBs database.
    /// </summary>
    [Table("authors")]
    public class Author
    {
        [Key]
        [Column("au_id")]
        [MaxLength(11)]
        public string AuId { get; set; } = string.Empty;

        [Column("au_lname")]
        [MaxLength(40)]
        public string AuLname { get; set; } = string.Empty;

        [Column("au_fname")]
        [MaxLength(20)]
        public string AuFname { get; set; } = string.Empty;

        [Column("phone")]
        [MaxLength(12)]
        public string Phone { get; set; } = string.Empty;

        [Column("address")]
        [MaxLength(40)]
        public string? Address { get; set; }

        [Column("city")]
        [MaxLength(20)]
        public string? City { get; set; }

        [Column("state")]
        [MaxLength(2)]
        public string? State { get; set; }

        [Column("zip")]
        [MaxLength(5)]
        public string? Zip { get; set; }

        [Column("contract")]
        public bool Contract { get; set; }

        // Navigation property
        public virtual ICollection<TitleAuthor> TitleAuthors { get; set; } = new List<TitleAuthor>();

        /// <summary>
        /// Gets the full name of the author.
        /// </summary>
        [NotMapped]
        public string FullName => $"{AuFname} {AuLname}";
    }
}
