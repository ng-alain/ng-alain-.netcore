using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace asdf.Core {
    public class ExceptionAttribute : ExceptionFilterAttribute {
        public override void OnException(ExceptionContext context) {
            var res = context.HttpContext.Response;

            res.StatusCode = 200;
            res.ContentType = "application/json; charset=utf-8";
            context.Result = new JsonResult(new {
                msg = context.Exception.Message,
                code = 503
            });
        }
    }
}