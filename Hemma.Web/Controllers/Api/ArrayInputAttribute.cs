using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Web.Http.Filters;

namespace Hemma.Web.Controllers.Api
{
    public class ArrayInputAttribute : ActionFilterAttribute
    {
        private readonly string _parameterName;

        public ArrayInputAttribute(string parameterName)
        {
            _parameterName = parameterName;
            Separator = ',';
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ActionArguments.ContainsKey(_parameterName))
            {
                string parameters = string.Empty;
                if (actionContext.ControllerContext.RouteData.Values.ContainsKey(_parameterName))
                    parameters = (string)actionContext.ControllerContext.RouteData.Values[_parameterName];
                else if (actionContext.ControllerContext.Request.RequestUri.ParseQueryString()[_parameterName] != null)
                    parameters = actionContext.ControllerContext.Request.RequestUri.ParseQueryString()[_parameterName];

                var values = parameters.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
                if (values == null || values.Count() == 0)
                    return;

                actionContext.ActionArguments[_parameterName] = values;
            }
        }

        public char Separator { get; set; }
    }
}