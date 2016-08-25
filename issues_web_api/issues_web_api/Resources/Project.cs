using System.Collections.Generic;
using issues_web_api.Resources.beans;

namespace issues_web_api.Resources
{
    public class Project : Resource
    {

        public Project(string id, string name)
        {
            Properties = new Properties()
            {
                Id = id,
                Name = name,
                Tags = new List<string>()
            };
        }
        
    }
}
