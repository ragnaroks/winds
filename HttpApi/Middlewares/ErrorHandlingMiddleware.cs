using HttpApi.MiddlewareEntities.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace HttpApi.Middlewares {
    /// <summary>未处理异常中间件</summary>
    public class ErrorHandlingMiddleware {
        /// <summary>日志</summary>
        private ILogger<ErrorHandlingMiddleware> Logger{get;}
        /// <summary>委托</summary>
        private RequestDelegate Next{get;}
        
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="logger">日志依赖</param>
        /// <param name="next">委托依赖</param>
        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger,RequestDelegate next) {
            this.Logger=logger;
            this.Next=next;
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="context">http 上下文</param>
        public async void Invoke(HttpContext context){
            try{
                await this.Next(context).ConfigureAwait(false);
            }catch(Exception exception){
                Logger.LogError(exception,"一个未处理的异常已被捕获");
                context.Response.StatusCode=StatusCodes.Status400BadRequest;
                context.Response.Headers.Add("winds-trace-identifier",context.TraceIdentifier);
                context.Response.ContentType="application/json";
                Response response=new Response().SetErrorType(1).SetErrorMessage(exception.Message).SetReadableErrorMessage($"请求 {context.TraceIdentifier} 遇到一个未处理的错误，您的此次请求数据已被记录，请稍后再试").SetStackTrace(exception.StackTrace);
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
