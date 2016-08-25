using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace issues_web_api.DataAccess.Models
{
    public class CommentModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

		[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public DateTime CreationDate { get; set; }

        [Key, Column(Order = 1)]
        public virtual IssueModel IssueModel { get; set; }
        [Key, Column(Order = 2)]
        public virtual ProjectModel ProjectModel { get; set; }

    }
}
