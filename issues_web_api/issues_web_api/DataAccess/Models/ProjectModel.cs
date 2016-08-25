using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace issues_web_api.DataAccess.Models
{
   public class ProjectModel 
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string Name { get; set; }

        //public virtual List<TagModel> TagsList { get; set; } //n tags
        
        public List<IssueModel> IssuesList { get; set; } //n issues

    }
}
