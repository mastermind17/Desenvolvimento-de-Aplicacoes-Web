
using System.Net;
using System.Net.Http;
using System.Web.Http;
using issues_web_api.Controllers;
using Xunit;

namespace issues_web_api.tests.controllers
{
    public class IssuesControllerTest
    {
        private HttpResponseMessage RequestSingleIssue(string projectName, int issueId)
        {
            var ctrler = new IssuesController
            {
                Configuration = new HttpConfiguration(),
                Request = new HttpRequestMessage()
            };
            return ctrler.GetSingleIssue(projectName, issueId).Result;
        }

        [Fact]
        public void RequestInexistentIssue_FromInvalidProject_CodeShouldBe404()
        {
            Assert.Equal(HttpStatusCode.NotFound, RequestSingleIssue("projectname", -1).StatusCode);
        }

        [Fact]
        public void RequestInexistentIssue_FromValidProject_CodeShouldBe404()
        {
            Assert.Equal(HttpStatusCode.NotFound, RequestSingleIssue("Phase Two", 0).StatusCode);
        }

        [Fact]
        public void RequestInexistentIssue_MedyatypeShouldBeProblemPlusJson()
        {
            Assert.Equal("application/problem+json", RequestSingleIssue("Phase One", 0).Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async void AddTagToIssue_CodeShouldBe204()
        {
            var issueCtrlPost = new IssuesController();
            issueCtrlPost.Request = new HttpRequestMessage() { Method = HttpMethod.Post };

            //TODO estes dados tem de existir de alguma forma
            string projectName = "Aplicacao-movel-via-verde", tagName = "bug";
            int id = 16;
            var response = await issueCtrlPost.AssociationBetweenTagAndIssue(projectName, id, tagName);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //repor dados com DELETE da associação criada anteriormente
            var issueCtrlDelete = new IssuesController();
            issueCtrlDelete.Request = new HttpRequestMessage() {Method = HttpMethod.Delete};
            var deleteResp = await issueCtrlDelete.AssociationBetweenTagAndIssue(projectName, id, tagName);
            Assert.Equal(HttpStatusCode.OK, deleteResp.StatusCode);

            //BUG ha aqui um .. O delete não executa, lança exception, possivel race-condition
        }


    }
}
