using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace issues_web_api.DataAccess.Models
{
    public class IssueTagSetModel
    {
        [Key, ForeignKey("IssueModel"), Column(Order = 0)]
        public int IssueId { get; set; }

        //navigation properties (nao sao para ser usadas em código)
        public virtual IssueModel IssueModel{ get; set; }

        [Key, ForeignKey("TagModel"), Column(Order = 1)]
        public string TagName { get; set; }

        //navigation properties (nao sao para ser usadas em código)
        public virtual TagModel TagModel { get; set; }
    }
}
