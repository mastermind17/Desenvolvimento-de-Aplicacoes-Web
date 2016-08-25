using System.Collections.Generic;

namespace issues_web_api.Resources.beans
{
    public class Action
    {
        
        public string Name { get; set; }
        public string Title { get; set; }  
        public string Method { get; set; }
        public string Href { get; set; }
        public string Type { get; set; }
        public List<Dictionary<string, string>> Fields { get; set; }
    }
}
