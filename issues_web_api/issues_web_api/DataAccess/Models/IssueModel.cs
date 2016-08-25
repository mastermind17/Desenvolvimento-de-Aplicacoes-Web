using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace issues_web_api.DataAccess.Models
{
    public class IssueModel
    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Key, Column(Order = 1)]
        public virtual ProjectModel ProjectModel { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string Description { get; set; }
        
		public List<CommentModel> Comments { get; set; } //N comments


    }
}
