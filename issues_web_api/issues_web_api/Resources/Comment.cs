using System;
using issues_web_api.Resources.beans;

namespace issues_web_api.Resources
{
	class Comment : Resource
	{
		public Comment(string id, string content, DateTime creationDate)
		{
			Properties = new Properties()
			{
				Id = id,
				Content = content,
				CreationDate = creationDate
			};
		}
	}
}
