using HttpApi.ControllerEntities.Default;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Reflection;

namespace HttpApi.Controllers {
    public class DefaultController:Controller {
        [ResponseCache(Duration=86400,Location=ResponseCacheLocation.Client)]
        public JsonResult Index() {
            String version=Assembly.GetEntryAssembly().GetName().Version.ToString();
            String ipAddress=Helpers.HttpRequestHelper.GetClientIp(Request);
            String userAgent="unkown";
            if(Request.Headers.ContainsKey("user-agent") && Request.Headers["user-agent"].Count>0){ userAgent=Request.Headers["user-agent"][0]; }
            String origin="unkown";
            if(Request.Headers.ContainsKey("origin") && Request.Headers["origin"].Count>0){ userAgent=Request.Headers["origin"][0]; }
            IndexResponse response=new IndexResponse().SetVersion(version).SetIpAddress(ipAddress).SetUserAgent(userAgent).SetOrigin(origin);
            return Json(response);
        }

        [ResponseCache(Duration=0,Location=ResponseCacheLocation.None,NoStore=true)]
        public JsonResult Error() {
            ErrorResponse response=new ErrorResponse().SetErrorType(1).SetErrorMessage("请求错误").SetReadableErrorMessage("请求遇到一个错误，已记录此错误并将尽快解决").SetTraceIdentifier(HttpContext.TraceIdentifier);
            if(Activity.Current!=null){ response.SetActivityCurrentId(Activity.Current.Id); }
            return Json(response);
        }
    }
}
