using System.Collections.Generic;
using System.Linq;
using issues_web_api.Resources.beans;

namespace issues_web_api.Resources
{
    /// <summary>
    /// This class represents the resources that follow
    /// the Siren specification.
    /// </summary>
    public abstract class Resource
    {
        protected Resource()
        {
            Class = new List<string>();
            Entities = new List<Entity>();
            Actions = new List<Action>();
            Links = new List<Link>();
        }


        public List<string> Class { get; private set; }
        public Properties Properties { get; set; }

        public List<Entity> Entities { get; private set; }

        public List<Action> Actions { get; private set; }

        public List<Link> Links { get; private set; }

        /// <summary>
        /// Helper method that adds a new link to the links collection.
        /// </summary>
        public void CreateNewLink(string[] rels, string href)
        {
            Links.Add(new Link
            {
                Rel = rels.ToList(),
                Href = href
            });
        }
        
        /*
        Estes métodos são utilizados pelo pacote JSON.NET para verificar 
        sobre que condições deve realizar o "parsing" de uma dada propriedade.

            http://www.newtonsoft.com/json/help/html/ConditionalProperties.html

        Estes métodos de verificação devem seguir o padrão "ShouldSerialize{property name}"
         */
        public bool ShouldSerializeActions() => Actions.Count > 0;
        public bool ShouldSerializeEntities() => Entities.Count > 0;
        public bool ShouldSerializeClass() => Class.Count > 0;

    }
}