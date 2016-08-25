using System.Data.Entity;
using issues_web_api.DataAccess.Models;

namespace issues_web_api.DataAccess
{
    public class ApiDbContext : DbContext
    {
        public DbSet<IssueModel> Issues { get; set; }

        public DbSet<ProjectModel> Projects { get; set; }

        public DbSet<CommentModel> Comments { get; set; }

        public DbSet<TagModel> Tags { get; set; }
        
        public DbSet<ProjectTagSetModel> ProjectTagSet { get; set; }

        public DbSet<IssueTagSetModel> IssueTagSet { get; set; }

    }

}
