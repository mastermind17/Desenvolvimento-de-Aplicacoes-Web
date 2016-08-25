
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using CollectionJson;
using Drum;
using issues_web_api.DataAccess;
using issues_web_api.filters;
using issues_web_api.Resources;
using issues_web_api.Resources.beans;
using Action = issues_web_api.Resources.beans.Action;
using Link = CollectionJson.Link;

namespace issues_web_api.Controllers
{
    [Logger]
    [EnableCors(origins: "http://localhost:8000", headers: "*", methods: "*")]
    public abstract class BaseController : ApiController
    {
        /// <summary>
        /// Gives access to the tables inside the DB.
        /// </summary>
        protected readonly ApiDbContext Context;
        
        /// <summary>
        /// The mediatype to be used when representing the state of a 
        /// single resource following the Siren specification.
        /// </summary>
        public const string SingleResourceMediatype = "application/vnd.siren+json";

        /// <summary>
        /// The mediatype to be used when representing the state of a 
        /// group of resources. Follows the collection+json specification.
        /// </summary>
        public const string CollectionResourceMediatype = "application/vnd.collection+json";

        public int PageSize { get; private set; }

        protected BaseController()
        {
            PageSize = 10;
            Context = new ApiDbContext();
        }

        protected BaseController(ApiDbContext ctx)
        {
            PageSize = 10;
            Context = ctx ?? new ApiDbContext();
        }


        protected Uri MakeUri<T>(Expression<Func<T, object>> uriGenerator) where T : BaseController
        {
            return Request.TryGetUriMakerFor<T>().UriFor(uriGenerator);
        }

        
        protected static Dictionary<string, string> CreateNewField(string name, string type, string value = null)
        {
            var fields = new Dictionary<string, string>
            {
                ["name"] = name,
                ["type"] = type
            };
            if (value != null)
            {
                fields.Add("value", value);
            }
            return fields;
        }

        protected virtual bool IsTemplateIncorrect(Template template)
        {
            if (template?.Data == null)
                return true;
            return (template.Data.Count < GetTemplateParams());
        }

        protected HttpResponseMessage CollectionTemplateInvalidResponse()
        {
            var invalidParams = new List<ErrorResource.InvalidParams> {
                new ErrorResource.InvalidParams
            {
                Name = "template",
                Reason = "Insertion template was not respected"
            }};
            return Request.BadRequestMessage(invalidParams);
        }

        protected abstract int GetTemplateParams();

		/// <summary>
		/// Add a related entity to the given resource's representation. This entity is the collection
		/// of issues. 
		/// </summary>
		protected static void AddEntity(List<string> classNames, Resource resource, string rel, Uri href)
		{
			resource.Entities.Add
			(
				new Entity()
				{
					Class = classNames,
					Rel = new List<string> { rel },
					Href = href.AbsoluteUri
				}
			);
		}

        /// <summary>
        /// Setup an action object that can be used to add new issues
        /// to this project.
        /// </summary>
        protected void SetupNewAction(Resource resource,
            string name,
            string title,
            string opaqueUri,
            List<Dictionary<string, string>> fields = null,
            string method = "POST",
            string type = "application/x-www-form-urlencoded")
        {
            resource.Actions.Add(new Action
            {
                Name = name,
                Title = title,
                Method = method,
                Href = opaqueUri,
                Type = type,
                Fields = fields
            });
        }

        protected void SetupPaginationLinks(Collection collection,int totalElems, int page, Uri previousPageUri, Uri nextPageUri)
        {

            var nextPageLink = SetupNextPageLink(totalElems, page, nextPageUri);
            if (nextPageLink != null)
            {
                collection.Links.Add(nextPageLink);
            }

            var prevPageLink = SetupPreviousPageLink(page, previousPageUri);
            if (prevPageLink != null)
            {
                collection.Links.Add(prevPageLink);
            }
        }


        protected Link SetupPreviousPageLink(int page, Uri previousPageUri)
        {
            if (page > 0)
            {
                return new Link
                {
                    Name = "previous_page",
                    Prompt = "Previous Page",
                    Rel = "previous",
                    Href = previousPageUri,
                    Render = "link"
                };
            }
            return null;
        }

        protected Link SetupNextPageLink(int totalElems, int page, Uri nextPageUri)
        {
            var totalPages = (totalElems - 1) / PageSize;

            if (totalPages > page)
            {
                return new Link
                {
                    Name = "next_page",
                    Prompt = "Next Page",
                    Rel = "next",
                    Href = nextPageUri,
                    Render = "link"
                };
            }
            return null;
        }

        protected async Task<bool> IsIssueRelatedToProject(string projectName, int issueId)
        {
            return await Context.Issues.AnyAsync(i => i.ProjectModel.Name.Equals(projectName) && (i.Id == issueId));
        }

        protected Resource BuildSirenResource(Resource resource,
            string resourceClass,
            Action<Resource> createAction,
            Action<Resource> createEntities,
            Action<Resource> createLinks)
        {
            var resourceBuilder = resource;
            resourceBuilder.Class.Add(resourceClass);
            
            //create Actions
            createAction(resourceBuilder);

            //create entities
            createEntities(resourceBuilder);

            //create links
            createLinks(resourceBuilder);

            return resourceBuilder;
        }

        protected DateTime ParseDate(string dateStr)
        {
            return DateTime.ParseExact(dateStr, "MM/dd/yyyy", CultureInfo.InvariantCulture);
        }


    }
}
