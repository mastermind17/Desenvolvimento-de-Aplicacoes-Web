using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
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
  [RoutePrefix("api/projects/{projectName}/issues/{issueId}/comments")]
  [MapHomeRelation("comments_url", "api/projects/{projectName}/issues/{issueId}/comments{/commentId}")]
  [IssueIdentifierValidator]
  public class CommentsController : BaseController
  {
    private const int NumberOfTemplateParameters = 1;

    protected override int GetTemplateParams()
    {
      return NumberOfTemplateParameters;
    }


    [Route("")]
    [DateRangeValidator]
    [HttpGet]
    public async Task<HttpResponseMessage> GetComments(string projectName, int issueId, [FromUri] string before = "",
      [FromUri] string after = "")
    {
      DateTime? beforeDate = null;
      DateTime? afterDate = null;

      if (!string.IsNullOrEmpty(before) && !string.IsNullOrWhiteSpace(before))
      {
        beforeDate = ParseDate(before);
      }
      if (!string.IsNullOrEmpty(after) && !string.IsNullOrWhiteSpace(after))
      {
        afterDate = ParseDate(after);
      }


      var collectionSelfUri = MakeUri<CommentsController>(c => c.GetComments(projectName, issueId, null, null));
      //root object
      var allCommentsCollection = new Collection {Version = "1.0", Href = collectionSelfUri};

      //DbFunctions.TruncateTime(someDateTimeObj) é igual a "someDateTimeObj.Date" mas LINQ não deixa utilizar esta ultima.
      var allCommentsTmp = Context.Comments
        .Where(c => c.IssueModel.Id == issueId
                    && c.ProjectModel.Name.Equals(projectName));


      if (afterDate != null)
      {
        allCommentsTmp =
          allCommentsTmp.Where(c => DbFunctions.TruncateTime(c.CreationDate) > DbFunctions.TruncateTime(afterDate));
      }
      if (beforeDate != null)
      {
        allCommentsTmp =
          allCommentsTmp.Where(c => DbFunctions.TruncateTime(c.CreationDate) < DbFunctions.TruncateTime(beforeDate));
      }

      var allComments = await allCommentsTmp.ToListAsync();
      foreach (var com in allComments)
      {
        var particularCommentUri =
          MakeUri<CommentsController>(c => c.GetSingleComment(projectName, com.IssueModel.Id, com.Id));
        var item = new Item {Href = particularCommentUri};
        item.Data.Add(new Data
        {
          Name = "created_on",
          Value = com.CreationDate.ToString(CultureInfo.InvariantCulture),
          Prompt = "Creation date"
        });
        item.Data.Add(new Data {Name = "content", Value = com.Content, Prompt = "The message"});
        allCommentsCollection.Items.Add(item);
      }

      var template = allCommentsCollection.Template.Data;
      template.Add(new Data {Name = "Content", Prompt = "content of comment"});

      allCommentsCollection.Queries.Add(new Query
      {
        Href = collectionSelfUri,
        Rel = Rels.Query.SearchQueryName,
        Prompt = "Search for comments within a certain date range.",
        Data = new List<Data>
        {
          new Data {Name = Rels.Search.After, Prompt = "Comments created after the given date."},
          new Data {Name = Rels.Search.Before, Prompt = "Comments created before the given date."}
        }
      });

      var document = new ReadDocument {Collection = allCommentsCollection};
      return Request.SetupResponse<IReadDocument>(HttpStatusCode.OK, document, CollectionResourceMediatype);
    }

    /// <summary>
    /// Restringe a lista de comentários de acordo com a indicação passada por parâmetro.
    /// A indicação será utilizada para obter um valor vindo do mapa de query strings do pedido.
    /// Caso não exista nenhum par chave-valor com esta indicação como chave, o método devolve a lista inicial
    /// sem ser modificada.
    /// </summary>
    private List<CommentModel> RestrictCommentsSet(List<CommentModel> allComments, string restrictRelation)
    {
      string dateValue;
      if (Request.TryGetQueryString(restrictRelation, out dateValue))
      {
        var date = ParseDate(dateValue);
        return allComments.Where(c => c.CreationDate.CompareTo(date) <= 0).ToList();
      }
      return allComments;
    }


    [Route("{commentId}")]
    public async Task<HttpResponseMessage> GetSingleComment(string projectName, int issueId, int commentId)
    {
      var thatComment = await Context.Comments.FindAsync(commentId);
      if (thatComment == null || thatComment.IssueModel.Id != issueId)
      {
        return Request.ResourceNotFoundMessage();
      }
      var isRelatedToProject = Context.Issues.Any(i => i.Id == issueId && i.ProjectModel.Name.Equals(projectName));
      if (!isRelatedToProject)
      {
        return Request.ResourceNotFoundMessage();
      }

      //construir o recurso Siren
      var resource = BuildCommentResource(projectName, issueId, thatComment);
      return Request.SetupResponse<Resource>(HttpStatusCode.OK, resource, SingleResourceMediatype);
    }


    [Route("")]
    [HttpPost]
    public async Task<HttpResponseMessage> CreateComment(string projectName, int issueId, WriteDocument template)
    {
      //assert template
      if (IsTemplateIncorrect(template.Template))
      {
        return CollectionTemplateInvalidResponse();
      }
      //assert association between project and issue
      if (!await IsIssueRelatedToProject(projectName, issueId))
      {
        return Request.ResourceNotFoundMessage();
      }
      //assert issue is not "closed"
      var issue =
        await Context.Issues.SingleOrDefaultAsync(i => i.ProjectModel.Name.Equals(projectName) && i.Id == issueId);
      if (issue.State.Equals(IssuesController.ClosedState))
      {
        var details = "A closed issue does not accept comments.";
        return Request.BadRequestMessage(null, details);
      }

      var contentOfComment = template.Template.Data[0].Value;
      var project = await Context.Projects.FindAsync(projectName);
      Context.Comments.Add(new CommentModel
      {
        Content = contentOfComment,
        ProjectModel = project,
        IssueModel = issue
      });
      await Context.SaveChangesAsync();

      // build the (siren) resource
      var lastIndex = Context.Comments.Where(c => c.IssueModel.Id == issueId).Max(i => (int?) i.Id);
      var model = await Context
        .Comments.SingleOrDefaultAsync(c => c.Id == lastIndex.Value
                                            && c.IssueModel.Id == issueId
                                            && c.ProjectModel.Name.Equals(projectName));

      var resource = BuildCommentResource(projectName, issueId, model);
      var selfLink = MakeUri<CommentsController>(c => c.GetSingleComment(projectName, issueId, model.Id));
      return Request.BuildCreatedResourceResponse(resource, selfLink);
    }

    [Route("{commentId}")]
    [HttpPut]
    public async Task<HttpResponseMessage> ModifyComment(string projectName, int id, int commentId)
    {
      return null;
    }

    private Resource BuildCommentResource(string projectName, int issueId, CommentModel model)
    {
      var comment = new Comment(model.Id.ToString(), model.Content, model.CreationDate);
      return BuildSirenResource(comment,
        Rels.SingleCommentRelationType,
        res =>
        {
          var modifyResourceUri = MakeUri<CommentsController>(c => c.ModifyComment(projectName, issueId, model.Id));
          SetupNewAction(res, "modify", "Modify Comment", modifyResourceUri.AbsoluteUri, null, HttpMethod.Put.ToString(),
            null);
        },
        res =>
        {
          //add top level entity (issue)
          var issueUri = MakeUri<IssuesController>(c => c.GetSingleIssue(projectName, issueId));
          AddEntity(new List<string> {Rels.SingleIssueRelationType}, res, Rels.SingleIssueRelationType, issueUri);
        },
        res =>
        {
          var selfLink = MakeUri<CommentsController>(c => c.GetSingleComment(projectName, issueId, model.Id));
          res.CreateNewLink(new[] {Rels.SelfRelationType}, selfLink.AbsoluteUri);
        });
    }
  }
}
