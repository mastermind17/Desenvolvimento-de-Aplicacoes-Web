using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using issues_web_api.Resources;

namespace issues_web_api.Controllers
{
    public static class ControllerExtensions
    {
        public static HttpResponseMessage BuildCreatedResourceResponse(this HttpRequestMessage request,
            Resource resource, Uri locationHeaderUri, string mediatype = BaseController.SingleResourceMediatype)
        {
            //send 201, representation and location of the new resource
            var response = request.CreateResponse(HttpStatusCode.Created, resource);
            response.Headers.Location = locationHeaderUri;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediatype);
            return response;
        }


        public static bool TryGetQueryString(this HttpRequestMessage request, string key, out string value)
        {
            var queryStrings = request.GetQueryNameValuePairs();
            if (queryStrings == null)
            {
                value = "";
                return false;
            }

            //OrdinalIgnoreCase é por causa da "culture". sugerido pelo resharper
            var match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, key, StringComparison.Ordinal) == 0);
            if (string.IsNullOrEmpty(match.Value))
            {
                value = "";
                return false;
            }

            value = match.Value;
            return true;
        }


        public static HttpResponseMessage SetupResponse<T>(this HttpRequestMessage request, HttpStatusCode statusCode, T content, string contentType)
        {
            //var response = new HttpResponseMessage(statusCode);
            if (content == null)
            {
                return request.CreateResponse(statusCode);
            }
            var response = request.CreateResponse(statusCode, content);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return response;
        }
    }
}