using issues_web_api.Resources.beans;

namespace issues_web_api.Resources
{
    public class Tag : Resource
    {
        
        public Tag(string id, string name)
        {
            Properties = new Properties()
            {
                Id = id,
                Name = name
            };
        }
    }
}
