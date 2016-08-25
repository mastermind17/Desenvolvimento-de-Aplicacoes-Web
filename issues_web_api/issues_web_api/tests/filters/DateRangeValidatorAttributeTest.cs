using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using issues_web_api.filters;
using Xunit;

namespace issues_web_api.tests.filters
{
    public class DateRangeValidatorAttributeTest
    {
        private const string NotValidParameter = "http://localhost/api/projects?after=something_else_than_a_date";
        private const string NotValidFormat = "http://localhost/api/projects?after=2016/04/23";
        private const string ValidFormat = "http://localhost/api/projects?after=04/23/2016";

        private readonly DateRangeValidatorAttribute _filter;

        public DateRangeValidatorAttributeTest()
        {
            _filter = new DateRangeValidatorAttribute();
        }
        HttpActionContext SetupContext(string uri)
        {
            return ContextUtil.CreateActionContext(ContextUtil.CreateControllerContext(request: new HttpRequestMessage(HttpMethod.Get, uri)));
        }


        [Fact]
        public void WrongDateParameter_ShouldReturn400()
        {
            var ctx = SetupContext(NotValidParameter);

            _filter.OnActionExecuting(ctx);

            Assert.NotNull(ctx.Response);
            Assert.Equal(HttpStatusCode.BadRequest, ctx.Response.StatusCode);
        }

        [Fact]
        public void WrongDateParameter_ShouldReturnProblemJsonMediatype()
        {
            var ctx = SetupContext(NotValidFormat);

            _filter.OnActionExecuting(ctx);

            Assert.NotNull(ctx.Response);
            Assert.Equal("application/problem+json", ctx.Response.Content.Headers.ContentType.MediaType);
        }


        [Fact]
        public void WrongDateFormat_ShouldReturn400()
        {
            var ctx = SetupContext(NotValidFormat);

            _filter.OnActionExecuting(ctx);

            Assert.NotNull(ctx.Response);
            Assert.Equal(HttpStatusCode.BadRequest, ctx.Response.StatusCode);
        }

        [Fact]
        public void WrongDateFormat_ShouldReturnProblemJsonMediatype()
        {
            var ctx = SetupContext(NotValidFormat);

            _filter.OnActionExecuting(ctx);

            Assert.NotNull(ctx.Response);
            Assert.Equal("application/problem+json", ctx.Response.Content.Headers.ContentType.MediaType);
        }


        [Fact]
        public void ValidDateFormat_ShouldHaveNullResponse()
        {
            var ctx = SetupContext(ValidFormat);

            _filter.OnActionExecuting(ctx);

            Assert.Null(ctx.Response);
        }


    }
}
