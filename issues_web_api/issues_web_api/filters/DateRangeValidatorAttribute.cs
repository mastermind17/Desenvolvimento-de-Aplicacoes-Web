using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using issues_web_api.Controllers;
using issues_web_api.Resources;

namespace issues_web_api.filters
{
    public class DateRangeValidatorAttribute : ActionFilterAttribute
    {
        private const string BeforeKey = "before";
        private const string AfterKey = "after";

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var queryPairs = actionContext.Request.GetQueryNameValuePairs().ToDictionary(k => k.Key);

            if (queryPairs.Count <= 0) return;

            var isBeforeParamValid = ValidateQueryParameter(queryPairs, BeforeKey);
            var isAfterParamValid = ValidateQueryParameter(queryPairs, AfterKey);

            if (isBeforeParamValid && isAfterParamValid) return;

            //looking for not valid dates
            var invalidParams = new List<ErrorResource.InvalidParams>();
            if (!isBeforeParamValid)
            {
                invalidParams.Add(new ErrorResource.InvalidParams
                {
                    Name = "before", Reason = "The before query parameter is not a valid date."
                });
            }
            if (!isAfterParamValid) 
            {
                invalidParams.Add(new ErrorResource.InvalidParams
                {
                    Name = "after",
                    Reason = "The after query parameter is not a valid date."
                });
            }
            actionContext.Response = actionContext.Request.BadRequestMessage(invalidParams);
        }

        /// <summary>
        /// Verifica se os parametros passados por query string podem
        /// ser convertidos numa instancia de DateTime. Caso isso se
        /// verifique, é devolvido TRUE. Caso contrario, FALSE.
        /// Se nao existirem parâmetros é devolvido TRUE para que o
        /// pedido possa seguir o seu fluxo normal.
        /// </summary>
        private static bool ValidateQueryParameter(Dictionary<string, KeyValuePair<string, string>> queryPairs, string key)
        {
            try
            {
                KeyValuePair<string, string> pair;
                if (queryPairs.TryGetValue(key, out pair))
                {
                    var s = pair.Value.Split(' ')[0];
                    DateTime dummy = DateTime.ParseExact(s, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    return true;
                }
                return true;
            }
            catch (FormatException)
            {
                Console.WriteLine($"'{key}' parameter is not valid.");
                return false;
            }
        }
    }
}
