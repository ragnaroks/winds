using HttpApi.MiddlewareEntities.ErrorHandling;
using HttpApi.ModuleEnums.Logger;
using HttpApi.Modules;
using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace HttpApi.Middlewares {
    /// <summary>未处理异常中间件</summary>
    public class ErrorHandlingMiddleware:IMiddleware {
        /// <summary>日志模块</summary>
        private LoggerModule LoggerModule{get;}
        
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="loggerModule">日志模块依赖</param>
        public ErrorHandlingMiddleware(LoggerModule loggerModule)=>this.LoggerModule=loggerModule;

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="context">http 上下文</param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context,RequestDelegate next) {
            try{
                await next(context).ConfigureAwait(false);
            }catch(Exception exception){
                this.LoggerModule.Log(LogLevel.Error,"Middlewares.ErrorHandlingMiddleware.InvokeAsync",$"一个未处理的异常已被捕获，{exception.Message}，{exception.StackTrace}");
                context.Response.StatusCode=StatusCodes.Status400BadRequest;
                context.Response.Headers.Add("winds-trace-identifier",context.TraceIdentifier);
                context.Response.ContentType="application/json";
                Response response=new Response().SetErrorType(1).SetErrorMessage(exception.Message).SetReadableErrorMessage($"请求 {context.TraceIdentifier} 遇到一个未处理的错误，您的此次请求数据已被记录，请稍后再试").SetStackTrace(exception.StackTrace);
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
