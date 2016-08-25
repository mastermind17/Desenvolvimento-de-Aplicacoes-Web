using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CollectionJson;
using issues_web_api.DataAccess;
using issues_web_api.DataAccess.Models;
using issues_web_api.filters;
using issues_web_api.Resources;

namespace issues_web_api.Controllers
{
    [RoutePrefix("api/projects")]
    [MapHomeRelation("projects_url", "api/projects{/projectName}")]
    public class ProjectsController : BaseController
    {
        private const int TemplateParams = 2;

        public ProjectsController(): this(new ApiDbContext())
        {
        }

        public ProjectsController(ApiDbContext ctx): base(ctx)
        {
        }

        protected override int GetTemplateParams()
        {
            return TemplateParams;
        }

        /// <summary>
        ///     Get a representation of a collection of projects.
        /// </summary>
        [Route("")]
        public async Task<HttpResponseMessage> GetProjects(int page = 0, string containsName = "")
        {
            var document = new ReadDocument();

            var collectionSelfUri = MakeUri<ProjectsController>(c => c.GetProjects(page, containsName));

            //root object
            var allProjectsCollection = new Collection {Version = "1.0", Href = collectionSelfUri};

            //partial representation of all the projects
            const string idProperty = "id", nameProperty = "name";
            var allProjectsFound = Context.Projects.Where(p => p.Name.Contains(containsName)).OrderBy(p => p.Name);
            var projectsConsidered = await allProjectsFound.Skip(PageSize*page).Take(PageSize).ToListAsync();
            foreach (var project in projectsConsidered)
            {
                var particularProjectUri = MakeUri<ProjectsController>(c => c.FindSingleProject(project.Name));
                var item = new Item {Href = particularProjectUri};
                item.Data.Add(new Data {Name = idProperty, Value = project.Id.ToString(), Prompt = "ID of project"});
                item.Data.Add(new Data {Name = nameProperty, Value = project.Name, Prompt = "name of project"});
                allProjectsCollection.Items.Add(item);
            }

            //template to insert new project
            var template = allProjectsCollection.Template.Data;
            template.Add(new Data {Name = Rels.Template.ProjectsPropertyName, Prompt = "name of project"});
            template.Add(new Data {Name = Rels.Template.ProjectsPropertyTags, Prompt = "Each Tag's name separated by a plus sign"});

            //queries para obter uma lista de projetos onde expression está contido no nome
            allProjectsCollection.Queries.Add(new Query
            {
                Href = MakeUri<ProjectsController>(c => c.GetProjects(page, "")),
                Rel = "search",
                Prompt = "Search for projects with a name that contains the given expression.",
                Data = new List<Data> { new Data { Name = Rels.Search.ContainsName, Prompt = "The keyword to look for inside the names of each project." } }
            });

            
            SetupPaginationLinks(allProjectsCollection, allProjectsFound.Count(), page, previousPageUri(page), nextPageUri(page));

            document.Collection = allProjectsCollection;
            return Request.SetupResponse<IReadDocument>(HttpStatusCode.OK, document, CollectionResourceMediatype);
        }


        /// <summary>
        ///     Get a representation of a specific project for a given name.
        /// </summary>
        [Route("{projectName}")]
        [HttpGet]
        public async Task<HttpResponseMessage> FindSingleProject(string projectName)
        {
            //verificar se existe um projecto com este nome
            var projectFromDb = await Context.Projects.FindAsync(projectName);
            if (projectFromDb == null)
            {
                return Request.ResourceNotFoundMessage();
            }

            var projectResource = await BuildProjectResource(projectName, projectFromDb.Id.ToString());

            //setup response
            return Request.SetupResponse(HttpStatusCode.OK, projectResource, SingleResourceMediatype);
        }

        /// <summary>
        ///     For a given request with method DELETE,
        ///     remove a project with given name.
        /// </summary>
        [Route("{projectName}")]
        public async Task<HttpResponseMessage> DeleteProject(string projectName)
        {
            var projectFromDb = await Context.Projects.FindAsync(projectName);
            if (projectFromDb == null)
            {
                return Request.ResourceNotFoundMessage();
            }

            Context.Projects.Remove(projectFromDb);
            await Context.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }


        /// <summary>
        ///     The action used to answer to POST requests that want to create a new
        ///     project.
        ///     When this action is well-succeded the response returns an 201 HTTP status code
        ///     in order to show that the request has been fulfilled and has resulted in one
        ///     new resource being created.
        /// </summary>
        [Route("")]
        [HttpPost]
        [ValidateModelState]
        public async Task<HttpResponseMessage> CreateProject(WriteDocument template)
        {
            //model state nao funciona aqui porque não tenho [Required] no tipo WriteDocument
            if (IsTemplateIncorrect(template.Template))
            {
                return CollectionTemplateInvalidResponse();
            }

            const int projectNameIdx = 0;

            var projectName = template.Template.Data[projectNameIdx].Value ;
            if (projectName == null)
            {
                return CollectionTemplateInvalidResponse();
            }
            projectName = projectName.Replace(" ", "-");

            var existsProject = await Context.Projects.AnyAsync(p => p.Name.Equals(projectName));
            if (existsProject)
            {
                return Request.BadRequestMessage(new List<ErrorResource.InvalidParams>
                {
                    new ErrorResource.InvalidParams
                    {
                        Name = template.Template.Data[projectNameIdx].Name,
                        Reason = $"A project with the name '{projectName}' already exists."
                    }
                });
            }

            if (! await SaveNewProject(template, projectName))
            {
                return CollectionTemplateInvalidResponse();
            }

            //build siren resource's representation
            var dbRecord = await Context.Projects.FindAsync(projectName);
            var resource = await BuildProjectResource(dbRecord.Id.ToString(), dbRecord.Name);

            //send 201, representation and location of the new resource
            var locationUri = MakeUri<ProjectsController>(c => c.FindSingleProject(projectName));
            return Request.BuildCreatedResourceResponse(resource, locationUri);
        }

        /// <summary>
        ///     With given data collected from the request, builds an instance of ProjectModel
        ///     and inserts it into the DB.
        /// </summary>
        private async Task<bool> SaveNewProject(IWriteDocument template, string projectName)
        {
            const int tagsParamIndex = 1;
            var tags = template.Template.Data[tagsParamIndex].Value;
            if (tags == null)
            {
                return false;
            }
            var tagModels = new List<string>();
            if (tags.Length > 0)
            {
                tagModels = tags.Split('+').ToList();
            }
            var newProject = new ProjectModel {Name = projectName};
            Context.Projects.Add(newProject);

            //handle tags
            foreach (var tagModel in tagModels)
            {
                var found = Context.Tags.Find(tagModel);
                if (found == null)
                {
                    Context.Tags.Add(new TagModel {Name = tagModel});
                    AssociateProjectWithTag(newProject.Name, tagModel);
                }
                else
                {
                    AssociateProjectWithTag(newProject.Name, tagModel);
                }
            }

            await Context.SaveChangesAsync();
            return true;
        }

        private void AssociateProjectWithTag(string projectName, string tagName)
        {
            Context.ProjectTagSet.Add(new ProjectTagSetModel
            {
                ProjectName = projectName,
                TagName = tagName
            });
        }

        /// <summary>
        ///     Action de remover um dado recurso é apenas
        ///     um pedido com método DELETE para o URI
        ///     do próprio.
        /// </summary>
        private void SetupRemoveProjectAction(Resource projectResource, string uri)
        {
            SetupNewAction(projectResource, "delete", "Delete Project", uri, null, "DELETE", null);
        }

        private async Task<Resource> BuildProjectResource(string name, string id)
        {
            var projectResource = new Project(id, name);

            var resource = BuildSirenResource(projectResource,
                Rels.SingleProjectRelationType,
                res =>
                {
                    var opaqueUri = MakeUri<ProjectsController>(c => c.DeleteProject(name)).AbsoluteUri;
                    SetupRemoveProjectAction(res, opaqueUri);
                },
                res =>
                {
                    var projectIssuesUri = MakeUri<IssuesController>(c => c.GetIssues(name, "", "", 0));
                    AddEntity(new List<string> { Rels.CollectionIssuesRelationType, Rels.CollectionRelationType },
                        projectResource, Rels.CollectionIssuesRelationType, projectIssuesUri);
                    //list of tags
                    var projectTagsUri = MakeUri<TagController>(c => c.GetTags(name));
                    AddEntity(new List<string> { Rels.CollectionTagsRelationType, Rels.CollectionRelationType },
                        projectResource, Rels.CollectionTagsRelationType, projectTagsUri);
                },
                res =>
                {
                    var selfLink = MakeUri<ProjectsController>(c => c.FindSingleProject(name)).AbsoluteUri;
                    projectResource.CreateNewLink(new[] { Rels.SelfRelationType}, selfLink);
                });

            //add specfific element: tags
            var tagsRelatedToProject = await Context.ProjectTagSet.Where(p => p.ProjectName.Equals(name)).ToListAsync();
            foreach (var tag in tagsRelatedToProject)
            {
                resource.Properties.Tags.Add(tag.TagName);
            }

            return resource;
        }

        protected Uri previousPageUri(int page) => MakeUri<ProjectsController>(c => c.GetProjects(page - 1, ""));
        protected Uri nextPageUri(int page) => MakeUri<ProjectsController>(c => c.GetProjects(page + 1, ""));

    }
}
