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

namespace issues_web_api.Controllers
{
    [RoutePrefix("api/projects/{projectName}/tags")]
    [MapHomeRelation("tags_url", "api/projects/{projectName}/tags{/tagName}")]
    public class TagController : BaseController
    {
        private const int NumberOfTemplateParameters = 2;
        protected override int GetTemplateParams()
        {
            return NumberOfTemplateParameters;
        }

        [Route("")]
        public async Task<HttpResponseMessage> GetTags(string projectName)
        {
            var document = new ReadDocument();

            var collectionSelfUri = MakeUri<TagController>(c => c.GetTags(projectName));

            //root object
            var tagsCollection = new Collection { Version = "1.0", Href = collectionSelfUri };

            //partial rep of each tag
            var tags = await Context.ProjectTagSet.Where(t => t.ProjectName.Equals(projectName)).ToListAsync();
            foreach (var tag in tags)
            {
                var particularProjectUri = MakeUri<TagController>(c => c.GetTagByName(projectName, tag.TagName));
                var item = new Item { Href = particularProjectUri };
                item.Data.Add(new Data { Name = "name", Value = tag.TagName, Prompt = "name of tag" });
                tagsCollection.Items.Add(item);
            }

            //template to insert new project
            var template = tagsCollection.Template.Data;
            template.Add(new Data { Name = "name", Prompt = "name of tag" });
            template.Add(new Data { Name = "top_entity", Prompt = "name of project", Value = projectName });


            document.Collection = tagsCollection;
            return Request.SetupResponse<IReadDocument>(HttpStatusCode.OK, document, CollectionResourceMediatype);
        }

        [Route("{tagName}")]
        public async Task<HttpResponseMessage> GetTagByName(string projectName, string tagName)
        {
            //verificar se existe um projecto com este nome
            var projectFromDb = await Context.Projects.FindAsync(projectName);
            if (projectFromDb == null)
            {
                return Request.ResourceNotFoundMessage();
            }

            var projectResource = await BuildResource(projectName, tagName);

            //setup response
            return Request.SetupResponse(HttpStatusCode.OK, projectResource, SingleResourceMediatype);
        }

        [Route("{tagName}")]
        public async Task<HttpResponseMessage> Delete(string tagName)
        {
            var tag = await Context.Tags.FindAsync(tagName);
            if (tag == null)
            {
                return Request.ResourceNotFoundMessage();
            }

            Context.Tags.Remove(tag); //restantes registos são apagados por CascadeDelete fornecido pelo EF
            await Context.SaveChangesAsync();
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }


        [Route("")]
        [HttpPost]
        [ValidateModelState]
        public async Task<HttpResponseMessage> CreateTag(WriteDocument template)
        {
            //model state nao funciona aqui porque não existe [Required] no tipo WriteDocument
            if (IsTemplateIncorrect(template.Template))
            {
                return CollectionTemplateInvalidResponse();
            }

            var nameFromTemplate = template.Template.Data[0].Value;
            var projectNameFromTemplate = template.Template.Data[1].Value;

            nameFromTemplate = nameFromTemplate.Replace(" ", "-");
            var isTagPresent = await Context.Tags.AnyAsync(t => t.Name.Equals(nameFromTemplate));
            if (!isTagPresent)
            {
                Context.Tags.Add(new TagModel() { Name = nameFromTemplate });
            }
            var isTagRelatedToProject = await Context.ProjectTagSet.AnyAsync(p => p.TagName.Equals(nameFromTemplate));
            if (!isTagRelatedToProject)
            {
                Context.ProjectTagSet.Add(new ProjectTagSetModel
                {
                    ProjectName = projectNameFromTemplate,
                    TagName = nameFromTemplate
                });
            }
            else
            {
                return Request.BadRequestMessage(new List<ErrorResource.InvalidParams>
                {
                    new ErrorResource.InvalidParams
                    {
                        Name = template.Template.Data[0].Name,
                        Reason = $"A Tag with the name '{nameFromTemplate}' is already associated with the project '{projectNameFromTemplate}'."
                    }
                });
            }

            await Context.SaveChangesAsync();
            
            //build siren resource's representation to include in the response
            var dbRecord = await Context.Tags.FindAsync(nameFromTemplate);
            var resource = await BuildResource(projectNameFromTemplate, dbRecord.Name);

            var locationUri = MakeUri<ProjectsController>(c => c.FindSingleProject(nameFromTemplate));
            return Request.BuildCreatedResourceResponse(resource, locationUri);
        }


        private async Task<Tag> BuildResource(string projectName, string tagName)
        {
            //retrieve tags for this project
            var tag = await Context.Tags.FindAsync(tagName);

            var resource = new Tag(tag.Id.ToString(), tag.Name);
            resource.Class.Add(Rels.SingleTagRelationType);

            //create Actions
            var opaqueUri = MakeUri<TagController>(c => c.Delete(tagName)).AbsoluteUri;
            SetupRemoveTagAction(resource, opaqueUri);

            //entities
            //related project
            var relatedProject = MakeUri<ProjectsController>(c => c.FindSingleProject(projectName));
            AddEntity(new List<string> { Rels.SingleProjectRelationType }, resource, Rels.SingleProjectRelationType, relatedProject);

            //Links
            var selfLink = MakeUri<TagController>(c => c.GetTagByName(projectName, tagName)).AbsoluteUri;
            resource.CreateNewLink(new[] { Rels.SelfRelationType }, selfLink);
            return resource;
        }

        /// <summary>
        ///     Action de remover um dado recurso é apenas
        ///     um pedido com método DELETE para o URI
        ///     do próprio.
        /// </summary>
        private void SetupRemoveTagAction(Resource resource, string uri)
        {
            SetupNewAction(resource, "delete", "Delete Tag", uri, null, "DELETE", null);
        }
    }
}
