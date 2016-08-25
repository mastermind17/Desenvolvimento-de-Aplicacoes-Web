namespace issues_web_api.Controllers
{
    public sealed class Rels
    {

        public static readonly string CollectionIssuesRelationType = "issues";
        public static readonly string CollectionCommentsRelationType = "comments";
        public static readonly string CollectionTagsRelationType = "tags";

        public static readonly string CollectionRelationType = "collection";
        public static readonly string SelfRelationType = "self";
        
        public static readonly string SingleIssueRelationType = "issue";
        public static readonly string SingleCommentRelationType = "comment";
        public static readonly string SingleTagRelationType = "tag";
        public static readonly string SingleProjectRelationType = "project";

        public static class Search
        {
            public static readonly string ContainsName = "containsName";
            public static readonly string After = "after";
            public static readonly string Before = "before";
        }

        public static class Query
        {
            public static readonly string FilterQueryName = "filter";
            public static readonly string SearchQueryName = "search";
        }

        public static class Template
        {
            public static readonly string IssueTemplateStateName = "State";
            public static readonly string ProjectsPropertyName= "name";
            public static readonly string ProjectsPropertyTags = "tags";
        }

    }
}
