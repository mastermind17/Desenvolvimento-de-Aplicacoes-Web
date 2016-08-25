using System.Collections.Generic;

namespace issues_web_api.Resources.beans
{
    public class Entity
    {
        public List<string> Class { get; set; }
        public List<string> Rel { get; set; }
        public string Href { get; set; }
        public List<Link> Links { get; set; }
        public Properties Properties { get; set; }
    }
}
