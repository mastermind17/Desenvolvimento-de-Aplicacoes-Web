using System.Collections.Generic;
using issues_web_api.Resources.beans;

namespace issues_web_api.Resources
{
    public class Issue : Resource
    {
        public Issue(string id, string title, string description, string state="open")
        {
            Properties = new Properties()
            {
                Id = id,
                Title = title,
                State = state,
                Description = description,
                Tags = new List<string>()
            };
        }

    }
}
