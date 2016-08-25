using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using CollectionJson;
using Drum;
using issues_web_api.Controllers;
using Xunit;

namespace issues_web_api.tests.controllers
{
    public class ProjectsControllerTest
    {
        //isto está dependente do que existir na DB
        private const string ExistíngProjectName = "Aplicacao-movel-via-verde";
        private const string NotExistíngProjectName = "NomeQueNaoExiste";

        /// <summary>
        ///     Expect 400 - Bad Request When Trying To create a project
        ///     with a name that already exists.
        /// </summary>
        [Fact]
        public void CreateProjectBadTemplate_CodeShouldBe400()
        {
            var template = new Template();
            template.Data.Add(new Data {Name = "TagsRel", Value = "Bug"});
            var response = ProcessNewProject(template);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public void CreateProjectBadTemplate_ContentTypeShouldBeProblemJson()
        {
            var template = new Template();
            template.Data.Add(new Data {Name = "TagsRel", Value = "Bug"});
            var response = ProcessNewProject(template);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void RequestAllProjects_CodeShouldBe200()
        {
            Assert.Equal(HttpStatusCode.OK, RequestAllProjects().StatusCode);
        }

        [Fact]
        public void RequestAllProjects_ContentShouldNotBeEmpty()
        {
            Assert.NotEmpty(RequestAllProjects().Content.ToString());
        }


        [Fact]
        public void RequestInexistentProject_CodeShouldBe404()
        {
            Assert.Equal(HttpStatusCode.NotFound, RequestSingleProject(NotExistíngProjectName).StatusCode);
        }

        [Fact]
        public void RequestInexistentProject_ContentTypeShouldBeProblemJson()
        {
            Assert.Equal("application/problem+json",
                RequestSingleProject(NotExistíngProjectName).Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void RequestSingleProject_ContentTypeShouldBeSirenJson()
        {
            Assert.Equal("application/vnd.siren+json",
                RequestSingleProject(ExistíngProjectName).Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void RequestSingleProject_StatusCodeShouldBeOk()
        {
            Assert.Equal(HttpStatusCode.OK, RequestSingleProject(ExistíngProjectName).StatusCode);
        }



        private HttpResponseMessage RequestSingleProject(string name)
        {
            var uri = "http://localhost/api/projects/" + name;
            var ctrler = new ProjectsController
            {
                Configuration = new HttpConfiguration(),
                Request = new HttpRequestMessage(),
                ActionContext = ContextUtil.CreateActionContext(ContextUtil.CreateControllerContext(request: new HttpRequestMessage(HttpMethod.Get, uri)))
            };
            return ctrler.FindSingleProject(name).Result;
        }

        private HttpResponseMessage RequestAllProjects()
        {
            var uri = "http://localhost/api/projects/";
            var ctrler = new ProjectsController
            {
                Configuration = new HttpConfiguration(),
                Request = new HttpRequestMessage(),
                ControllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage(HttpMethod.Get, uri))
            };

            //configuração para a lib DRUM
            ctrler.Configuration.MapHttpAttributeRoutesAndUseUriMaker(new DefaultDirectRouteProvider());
            ctrler.Configuration.EnsureInitialized();

            //ctrler.Request.SetConfiguration(new HttpConfiguration());
            return ctrler.GetProjects(0).Result;
        }

        private HttpResponseMessage ProcessNewProject(Template newProjTemplate)
        {
            var ctrler = new ProjectsController
            {
                Configuration = new HttpConfiguration(),
                Request = new HttpRequestMessage()
            };
            var dataFromBody = new WriteDocument { Template = newProjTemplate };
            return ctrler.CreateProject(dataFromBody).Result;
        }

    }
}