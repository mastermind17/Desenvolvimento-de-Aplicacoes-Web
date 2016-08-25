using System;
using System.Collections.Generic;

namespace issues_web_api.Resources.beans
{
    public class Properties
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string State { get; set; }

        public List<string> Tags { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
		public string Content { get; set; }
		public DateTime CreationDate { get; set; }
	}
}
