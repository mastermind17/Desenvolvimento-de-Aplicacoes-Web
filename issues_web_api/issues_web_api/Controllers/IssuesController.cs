using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using CollectionJson;
using issues_web_api.DataAccess.Models;
using issues_web_api.filters;
using issues_web_api.Resources;
using Action = issues_web_api.Resources.beans.Action;

namespace issues_web_api.Controllers
{

    [RoutePrefix("api/projects/{projectName}/issues")]
    [MapHomeRelation("issues_url", "api/projects/{projectName}/issues{/issueId}")]
    public class IssuesController : BaseController
    {
        public static string OpenState = "open";
        public static string ClosedState = "closed";

        private const int TemplateParams = 3;

        protected override int GetTemplateParams()
        {
            return TemplateParams;
        }

        [Route("")]
        public async Task<HttpResponseMessage> GetIssues(string projectName, string state = "", string containsName = "", int page = 0)
        {
            var document = new ReadDocument();
            var collectionSelfUri = MakeUri<IssuesController>(c => c.GetIssues(projectName, null, null, 0));

            //root object
            var allIssuesCollection = new Collection{ Version = "1.0", Href = collectionSelfUri };
            IOrderedQueryable<IssueModel> allIssuesFound = null;

            if (state.Length > 0)
            {
                allIssuesFound = Context.Issues.Where(i =>
                    //to handle queries over the collection
                    (state.Length > 0)
                        ? i.ProjectModel.Name.Equals(projectName) && i.State.Equals(state)
                        : i.ProjectModel.Name.Equals(projectName)
                    ).OrderBy(i => i.Id);
            }
            else
            {
                allIssuesFound = Context.Issues.Where(i =>
                    //to handle queries over the collection
                    (containsName.Length > 0)
                        ? i.ProjectModel.Name.Equals(projectName) && i.Title.Contains(containsName)
                        : i.ProjectModel.Name.Equals(projectName)
                    ).OrderBy(i => i.Id);
            }
            var allIssuesConsidered = await allIssuesFound.Skip(PageSize * page).Take(PageSize).ToListAsync();

            //items
            foreach (var issue in allIssuesConsidered)
            {
                var particularIssueUri = MakeUri<IssuesController>(c => c.GetSingleIssue(projectName, issue.Id));
                var item = new Item { Href = particularIssueUri };
                item.Data.Add(new Data { Name = "title", Value = issue.Title, Prompt = "title of issue" });
                item.Data.Add(new Data { Name = "state", Value = issue.State, Prompt = "state of issue" });
                allIssuesCollection.Items.Add(item);
            }

            //template
            var template = allIssuesCollection.Template.Data;
            template.Add(new Data { Name = "title", Prompt = "title of issue" });
            template.Add(new Data { Name = "state", Prompt = "state of issue" });
            template.Add(new Data { Name = "description", Prompt = "description of issue" });
            template.Add(new Data { Name = "tags", Prompt = "Each tag's name, if any, separated by a plus sign." });

            //queries para obter uma lista de issues com um determinado state (open ou close)
            allIssuesCollection.Queries.Add(new Query
            {
                Href = collectionSelfUri,
                Rel = Rels.Query.FilterQueryName,
                Prompt = "Search for issues with a certain state.",
                Data = new List<Data> { new Data { Name = "state", Prompt = "The state to look for."} }
            });
            //queries para procurar por um issue que contenha um determinado nome
            allIssuesCollection.Queries.Add(new Query
            {
                Href = MakeUri<IssuesController>(c => c.GetIssues(projectName, "", "", page)),
                Rel = Rels.Query.SearchQueryName,
                Prompt = "Search for issues that contains the given expression.",
                Data = new List<Data> { new Data { Name = Rels.Search.ContainsName, Prompt = "The keyword to look for inside the names of each issue." } }
            });

            SetupPaginationLinks(allIssuesCollection, Context.Issues.Count(), page, previousPageUri(projectName, state, containsName, page), 
                nextPageUri(projectName, state, containsName, page));

            document.Collection = allIssuesCollection;
            return Request.SetupResponse<IReadDocument>(HttpStatusCode.OK, document, CollectionResourceMediatype);
        }

        [Route("{id}")]
        [IssueIdentifierValidator]
        public async Task<HttpResponseMessage> GetSingleIssue(string projectName, int id)
        {
            if (! await IsIssueRelatedToProject(projectName, id))
            {
                return Request.ResourceNotFoundMessage();
            }

            var thatIssue = await Context.Issues.FindAsync(id);

            //construir o recurso Siren
            var resource = await BuildIssueResource(projectName, thatIssue);
            return Request.SetupResponse(HttpStatusCode.OK, resource, SingleResourceMediatype);
        }


        [Route("")]
        [HttpPost]
        [ValidateModelState]
        public async Task<HttpResponseMessage> CreateIssue(string projectName, WriteDocument template)
        {
            var body = template.Template;
            if (IsTemplateIncorrect(body))
            {
                return CollectionTemplateInvalidResponse();
            }

            var projectRootEntity = await Context.Projects.FindAsync(projectName);
            if (projectRootEntity == null)
            {
                return Request.ResourceNotFoundMessage();
            }

            //BadRequest if state is not 'open' or 'closed'
            const int titleIdx = 0, stateIdx = 1, descriptionIdx = 2, tagsIdx = 3;
            if (!IsIssueStateValid(body.Data[stateIdx]?.Value))
            {
                return BadIssueStateErrorMessage(body, stateIdx);
            }

            int? lastElementIndex = Context.Issues.Max(i => (int?)i.Id); //used in the making of the URI

            //Tags must belong to the project's set
            if (template.Template.Data.Count > GetTemplateParams())
            {
                var tags = template.Template.Data[tagsIdx].Value.Split('+');
                var tagModels = tags.Select(tag => new TagModel {Name = tag}).ToList();
                //check if each tag is present in the ProjectTag set. if not, returns Error else associate it with the issue
                foreach (var tagModel in tagModels)
                {
                    //verificar existencia no project relativo a este issue
                    if (!await IsTagRelatedToProject(projectName, tagModel))
                    {
                        return TagNotRelatedToProjectError(body.Data[tagsIdx].Name);
                    }
                    //caso contrario, associa tag com issue
                    var elemIndex = (int)((lastElementIndex == null) ? 1 : lastElementIndex + 1);
                    AssociationBetweenTagAndIssue(tagModel, elemIndex);
                }
            }

            //what to save
            var newResource = new IssueModel
            {
                Title = body.Data[titleIdx].Value,
                State = body.Data[stateIdx].Value,
                Description = body.Data[descriptionIdx].Value,
                ProjectModel = projectRootEntity
            };
            Context.Issues.Add(newResource);

            Context.SaveChanges();

            var issueCreated = await BuildIssueResource(projectName, newResource);
            var locationUri = MakeUri<IssuesController>(c => c.GetSingleIssue(projectName, lastElementIndex.Value));
            return Request.BuildCreatedResourceResponse(issueCreated, locationUri);
        }

        private async Task<bool> IsTagRelatedToProject(string projectName, TagModel tagModel)
        {
            var isTagPresent =
                await Context.ProjectTagSet.AnyAsync(r => r.ProjectName.Equals(projectName) && r.TagName.Equals(tagModel.Name));
            return isTagPresent;
        }

        [Route("{id}")]
        [IssueIdentifierValidator]
        public async Task<HttpResponseMessage> DeleteIssue(string projectName, int id)
        {
            if (! await IsIssueRelatedToProject(projectName, id))
            {
                return Request.ResourceNotFoundMessage();
            }

            var issue = await Context.Issues.FindAsync(id);

            Context.Issues.Remove(issue);
            await Context.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Action que associa uma tag a um dado Issue.
        /// 
        /// Body do pedido deve conter um campo "tag" que contenha o nome
        /// da tag que pretende associar ao Issue. Issue é identificado
        /// pelo ID presente no Request URI.
        /// 
        /// Documentação diz o seguinte:
        /// 
        /// "Web API reads the response body at most once, so only one parameter 
        /// of an action can come from the request body. 
        /// If you need to get multiple values from the request body, define a complex type.
        /// Second, the client needs to send the value with the following format:
        ///  =value "
        /// 
        /// </summary>
        [Route("{id}/tags")]
        [HttpPost]
        [HttpDelete]
        [IssueIdentifierValidator]
        public async Task<HttpResponseMessage> AssociationBetweenTagAndIssue(string projectName, int id, [FromBody] string tag)
        {
            if (tag == null)
            {
                //lidar com erro
            }

            //verificar se tag está associada com Projecto ao qual o issue pertence
            var tagModel = new TagModel {Name = tag};
            if (! await IsTagRelatedToProject(projectName, tagModel))
            {
                return TagNotRelatedToProjectError("tag");
            }

            HttpResponseMessage response = null;
            //caso contrario, associar/remover conforme o HTTP method
            if (Request.Method.Equals(HttpMethod.Post))
            {
                //cria associação entre ambas
                AssociationBetweenTagAndIssue(tagModel, id);
                response = Request.CreateResponse(HttpStatusCode.NoContent);
            }
            else if (Request.Method.Equals(HttpMethod.Delete))
            {
                //elemina a associação
                RemoveAssociationBetweenTagAndIssue(tagModel, id);
                //if the resource is immediately removed, the server should return a 200 code
                response = Request.CreateResponse(HttpStatusCode.OK);
            }

            //gravar alterações
            await Context.SaveChangesAsync();
            return response;
        }

        

        private async Task<Resource> BuildIssueResource(string relatedProjectName, IssueModel modelInstance)
        {
            var issueResource = new Issue(modelInstance.Id.ToString(), modelInstance.Title, modelInstance.Description, modelInstance.State);

            var resource = BuildSirenResource(issueResource,
                Rels.SingleIssueRelationType,
                res =>
                {
                    var opaqueUri = MakeUri<IssuesController>(c => c.DeleteIssue(relatedProjectName, modelInstance.Id)).AbsoluteUri;
                    SetupRemoveIssueAction(res, opaqueUri);
                    //open/close action
                    var selfUri =
                        MakeUri<IssuesController>(c => c.GetSingleIssue(relatedProjectName, modelInstance.Id));
                    var isOpen = res.Properties.State.Equals(OpenState);
                    var action = isOpen
                        ? CloseIssueAction(selfUri)
                        : OpenIssueAction(selfUri);
                    res.Actions.Add(action);
                },
                res =>
                {
                    //list of comments
                    var commentsRelatedUri = MakeUri<CommentsController>(c => c.GetComments(relatedProjectName, modelInstance.Id, null, null));
                    AddEntity(new List<string>
                    {
                        Rels.CollectionCommentsRelationType, Rels.CollectionRelationType
                    }, res, Rels.CollectionRelationType, commentsRelatedUri);
                    //add top level entity (project)
                    var projectRelatedUri = MakeUri<ProjectsController>(c => c.FindSingleProject(relatedProjectName));
                    AddEntity(new List<string> { Rels.SingleProjectRelationType}, res, Rels.SingleProjectRelationType, projectRelatedUri);
                },
                res =>
                {
                    var selfLink = MakeUri<IssuesController>(c => c.GetSingleIssue(relatedProjectName, modelInstance.Id)).AbsoluteUri;
                    res.CreateNewLink(new[] { Rels.SelfRelationType }, selfLink);
                });

            //add specfific element: tags
            var tagsRelated = await Context.IssueTagSet.Where(p => p.IssueId.ToString().Equals(resource.Properties.Id)).ToListAsync();
            foreach (var tag in tagsRelated)
            {
                resource.Properties.Tags.Add(tag.TagName);
            }

            return resource;
        }

        /// <summary>
        ///     Action de remover um dado recurso é apenas
        ///     um pedido com método DELETE para o URI
        ///     do próprio.
        /// </summary>
        private void SetupRemoveIssueAction(Resource resource, string uri)
        {
            SetupNewAction(resource, "delete", "Delete Issue", uri, null, "DELETE", null);
        }

        private static Action CloseIssueAction(Uri self)
        {
            return ResolveIssueAction("close-issue", "Change issue's state to 'closed'", self);
        }

        private static Action OpenIssueAction(Uri self)
        {
            return ResolveIssueAction("open-issue", "Change issue's state to 'open'", self);
        }

        private static Action ResolveIssueAction(string actionName, string actionTitle, Uri self)
        {
            return new Action
            {
                Name = actionName,
                Title = actionTitle,
                //especificação indica este método como sendo o que modifica um recurso
                Method = "PUT",
                Href = self.AbsoluteUri,
                Type = "application/x-www-form-urlencoded",
                Fields = new List<Dictionary<string, string>>
                {
                    CreateNewField("id", "number"),
                    CreateNewField("title", "text"),
                    CreateNewField("state", "text"),
                    CreateNewField("description", "text")
                }
            };
        }

        [Route("{id}")]
        [HttpPut]
        [IssueIdentifierValidator]
        public async Task<HttpResponseMessage> ModifyIssueState(string projectName, int issueId, [FromBody] IssueModel modifiedIssue)
        {
            if (!ModelState.IsValid)
            {
                const string details = "The server was not able to make sense of the content inside the request body.";
                return Request.BadRequestMessage(null, details);
            }
            if (! await IsIssueRelatedToProject(projectName, issueId))
            {
                return Request.ResourceNotFoundMessage();
            }

            var issueToChange = await Context.Issues.FindAsync(issueId);
            issueToChange.State = modifiedIssue.State;
            await Context.SaveChangesAsync();
            return Request.SetupResponse(HttpStatusCode.OK, issueToChange, SingleResourceMediatype);
        }


        private HttpResponseMessage TagNotRelatedToProjectError(string invalidParamName)
        {
            var invalidParam = new List<ErrorResource.InvalidParams>
            {
                new ErrorResource.InvalidParams
                {
                    Name = invalidParamName,
                    Reason = "one or more tags are not associated with the project."
                }
            };
            const string detailsMsg =
                "Value must have tags that are associated with the project related to this issue.";
            return Request.BadRequestMessage(invalidParam, detailsMsg);
        }

        private HttpResponseMessage BadIssueStateErrorMessage(Template body, int stateIdx)
        {
            var invalidParam = new List<ErrorResource.InvalidParams>
            {
                new ErrorResource.InvalidParams
                {
                    Name = body.Data[stateIdx]?.Name ?? Rels.Template.IssueTemplateStateName,
                    Reason = "Value must exist and be either 'open' or 'closed'."
                }
            };
            const string detailsMsg = "An issue must have a state value of 'open' or 'closed'";
            return Request.BadRequestMessage(invalidParam, detailsMsg);
        }

        private void AssociationBetweenTagAndIssue(TagModel tagModel, int elementIndex)
        {
            Context.IssueTagSet.Add(new IssueTagSetModel { IssueId = elementIndex, TagName = tagModel.Name });
        }

        /// <summary>
        /// Remover funciona assim:
        ///  Temos de obter a referencia para o registo e 
        ///  indicar que queremos remover com aquela mesma referencia.
        /// 
        /// </summary>
        private async void RemoveAssociationBetweenTagAndIssue(TagModel tagModel, int id)
        {
            var reg = await Context.IssueTagSet.Where(item => item.IssueId == id && item.TagName.Equals(tagModel.Name)).FirstAsync();
            Context.IssueTagSet.Remove(reg);
        }

        private static bool IsIssueStateValid(string givenState) => givenState != null && (givenState.Equals(OpenState) || givenState.Equals(ClosedState));

        protected Uri previousPageUri(string projName, string state, string containsName, int page) => MakeUri<IssuesController>(c => c.GetIssues(projName, state, containsName, page - 1));
        protected Uri nextPageUri(string projName, string state, string containsName, int page) => MakeUri<IssuesController>(c => c.GetIssues(projName, state, containsName, page + 1));

    }
}
