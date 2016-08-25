using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace issues_web_api.filters
{
    public class LoggerAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var req = actionContext.Request;
            Console.WriteLine($"{req.Method.Method} - {req.RequestUri.AbsolutePath}");
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var resp = actionExecutedContext.Response;
            Console.WriteLine($"{resp.StatusCode}");
        }
    }
}
