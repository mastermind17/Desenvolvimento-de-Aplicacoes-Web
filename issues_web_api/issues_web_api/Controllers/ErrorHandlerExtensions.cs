using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using issues_web_api.Resources;

namespace issues_web_api.Controllers
{
    public static class ErrorHandlerExtensions
    {
        private const string ResourceNotFoundUri = "/api/doc#probls-res-not-found";

        /// <summary>
        /// Provides an error representation when some requested resource
        /// was not found.
        /// </summary>
        public static HttpResponseMessage ResourceNotFoundMessage(this HttpRequestMessage request)
        {
            const string errorSummary = "Resource Not Found";
            const string errorDetails = "It was not found any current representation for the target resource";

            var errorInstance = BuildErrorMessage(ResourceNotFoundUri, errorSummary, HttpStatusCode.NotFound,
                errorDetails);

            return SetupErrorMessage(request, HttpStatusCode.NotFound, errorInstance);
        }


        /// <summary>
        /// Provides an error representation when some received request
        /// cannot be processed due to something that is perceived to be a client error.
        /// </summary>
        public static HttpResponseMessage BadRequestMessage(this HttpRequestMessage request, 
            List<ErrorResource.InvalidParams> paramsExplanation, 
            string detail = null)
        {
            const string uriToThisError = "/api/doc#probls-bad-req";
            const string errorSummary = "The request parameters received are not correct.";

            var errorRep = BuildErrorMessage(uriToThisError, errorSummary, HttpStatusCode.BadRequest, 
                detail, paramsExplanation);

            return SetupErrorMessage(request, HttpStatusCode.BadRequest, errorRep);
        }

        private static HttpResponseMessage SetupErrorMessage(HttpRequestMessage request, 
            HttpStatusCode code, ErrorResource errorRep)
        {
            var resp = request.CreateResponse(code, errorRep);
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue(ErrorResource.ErrorProblemMediaType);
            return resp;
        }

        private static ErrorResource BuildErrorMessage(string uriToThisError, 
            string errorSummary, 
            HttpStatusCode code, 
            string detailsOfError,
            List<ErrorResource.InvalidParams> errorParams = null)
        {
            return new ErrorResource()
            {
                Type = uriToThisError,
                Title = errorSummary,
                Status = (int)code,
                Detail = detailsOfError,
                InvalidParames = errorParams
            };
        }
    }
}
