using System;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using issues_web_api.Controllers;
using issues_web_api.Resources;

namespace issues_web_api.filters
{
    public class IssueIdentifierValidator : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var model = actionContext.ModelState;
            try
            {
                ModelState idFromUri;
                model.TryGetValue("id", out idFromUri);
                if (idFromUri != null)
                {
                    var id = Convert.ToInt32(idFromUri.Value.AttemptedValue);
                }
            }
            catch (FormatException)
            {
                //Bad Request
                var invalidParams = new List<ErrorResource.InvalidParams>
                {
                    new ErrorResource.InvalidParams {Name = "id", Reason = "must be an integer"}
                };
                const string details = "The issue's id supplied is not a valid integer";
                actionContext.Response = actionContext.Request.BadRequestMessage(invalidParams, details);
            }
        }
    }
}
