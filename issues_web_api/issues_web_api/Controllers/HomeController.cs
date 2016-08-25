using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace issues_web_api.Controllers
{
  [RoutePrefix("")]
  public class HomeController : BaseController
  {

    /**
    * THIS IS WRONG. IM AWARE. 
    * In a web-development project the API must serve static files
    * accordingly with the configuration present inside the web.config file.
    * This is just here so it works out of the box.
    */
    [Route("{filename?}")]
    public async Task<HttpResponseMessage> GetWebClient(string filename = "index.html")
    {
      var currPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
      string filePath = "client\\" + filename;
      var documentationFileContent = await ReadTextAsync(Path.Combine(currPath, filePath));
      var response = new HttpResponseMessage
      {
        Content = new StringContent(documentationFileContent)
      };
      response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
      return response;
    }

    [Route("api")]
    public HttpResponseMessage Get()
    {
      var homeRelationsMap = MapLinkRelations();
      //content negotiation is performed by CreateResponse method
      var response = Request.CreateResponse(HttpStatusCode.OK, homeRelationsMap);
      response.Headers.CacheControl = new CacheControlHeaderValue()
      {
        //https://tools.ietf.org/html/draft-nottingham-json-home-03#section-6
        Public = true,
        MaxAge = new TimeSpan(0, 1, 0, 0)
      };
      return response;
    }


    [Route("doc")]
    public async Task<HttpResponseMessage> GetDocumentationPage()
    {
      var currPath = Directory.GetCurrentDirectory();
      const string filePath = @"doc.html";
      var documentationFileContent = await ReadTextAsync(Path.Combine(currPath, filePath));
      var response = new HttpResponseMessage
      {
        Content = new StringContent(documentationFileContent)
      };
      response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
      return response;
    }


    private async Task<string> ReadTextAsync(string filePath)
    {
      using (FileStream sourceStream = new FileStream(filePath,
        FileMode.Open, FileAccess.Read, FileShare.Read,
        bufferSize: 4096, useAsync: true))
      {
        StringBuilder sb = new StringBuilder();

        byte[] buffer = new byte[4096];
        int numRead;
        while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
        {
          string text = Encoding.UTF8.GetString(buffer, 0, numRead);
          sb.Append(text);
        }

        return sb.ToString();
      }
    }

    protected override int GetTemplateParams()
    {
      throw new NotImplementedException();
    }


    /*
          Pesquisa por controladores (sub-classes de BaseController) que tenham
          o atributo MapHomeRelationAttribute. Dentro deste atributo espera-se
          que esteja o nome da "link relation" e o valor da mesma.
         */

    private Dictionary<string, string> MapLinkRelations()
    {
      var dict = new Dictionary<string, string>();
      var reqUri = Request.RequestUri;
      var origin = $"{reqUri.Scheme}://{reqUri.Host}:{reqUri.Port}/";
      var classesList = Assembly.GetExecutingAssembly().GetTypes()
        .Where(t => t != typeof(HomeController) &&
                    t.IsSubclassOf(typeof(BaseController)))
        .ToList();

      foreach (var ctrl in classesList)
      {
        MapHomeRelationAttribute homeRelation = ctrl.GetCustomAttribute(typeof(MapHomeRelationAttribute))
          as MapHomeRelationAttribute;
        if (homeRelation == null) continue;
        var relationLinkUrl = origin + homeRelation.RelationValue;
        dict.Add(homeRelation.RelationName, relationLinkUrl);
      }
      return dict;
    }
  }
}
