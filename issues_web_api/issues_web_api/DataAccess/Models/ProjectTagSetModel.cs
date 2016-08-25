using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace issues_web_api.DataAccess.Models
{
    public class ProjectTagSetModel
    {
        [Key, ForeignKey("ProjectModel"), Column(Order = 0)]
        public string ProjectName { get; set; }

        //navigation properties (nao sao para ser usadas em código)
        public virtual ProjectModel ProjectModel { get; set; }  

        [Key, ForeignKey("TagModel"), Column(Order = 1)]
        public string TagName { get; set; }

        //navigation properties (nao sao para ser usadas em código)
        public virtual TagModel TagModel { get; set; }  
    }
}
