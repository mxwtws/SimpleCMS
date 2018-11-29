using SimpleCMS.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SimpleCMS.Models
{
    [Table("T_Content")]
    public class Content
    {
        [Key]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar")]
        public string Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column(TypeName = "nvarchar")]
        public string Title { get; set; }

        [Required]
        public string CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "nvarchar")]
        public string Image { get; set; }

        [MaxLength(500)]
        [Column(TypeName = "nvarchar")]
        public string Summary { get; set; }

        [Required]
        [Column(TypeName = "text")]
        public string Body { get; set; }

        [Required]
        [DefaultDateTimeValue("Now")]
        public DateTime? Created { get; set; }

        [Required]
        [DefaultValue(0)]
        public int Hits { get; set; }

        [Required]
        [Range(0, 1)]
        public byte State { get; set; }

        [Required]
        [DefaultValue(0)]
        public int SortOrder { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }

    }
}