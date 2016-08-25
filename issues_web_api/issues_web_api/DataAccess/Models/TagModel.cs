using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace issues_web_api.DataAccess.Models
{
    public class TagModel
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        public string Name { get; set; }

       //navigation properties 
        //public virtual List<ProjectModel> Projects { get; set; } //N projects

        //public virtual List<IssueModel> Issues { get; set; } //N issues
		
    }
}
