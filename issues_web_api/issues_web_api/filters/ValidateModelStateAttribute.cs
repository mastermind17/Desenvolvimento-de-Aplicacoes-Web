using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using issues_web_api.Controllers;

namespace issues_web_api.filters
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (IsContentEmpty(actionContext))
            {
                actionContext.Response = CreateErrorResponse(actionContext);
            }
        }

        private HttpResponseMessage CreateErrorResponse(HttpActionContext actionContext)
        {
            string details =
                "The body of your request is empty. The api cannot process empty POST requests for this URI.";
            return actionContext.Request.BadRequestMessage(null, details);
        }
        private bool IsContentEmpty(HttpActionContext actionCtx)
        {
            return actionCtx.Request.Content.Headers.ContentLength == 0;
        }

    }
}
