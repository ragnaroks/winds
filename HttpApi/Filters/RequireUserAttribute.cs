using HttpApi.FilterEntities.RequireUser;
using HttpApi.ModuleEntities.UserManage;
using HttpApi.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;

namespace HttpApi.Filters {
    /// <summary>用户权限过滤器</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RequireUserAttribute:Attribute, IAuthorizationFilter {
        /// <summary>用户管理模块依赖</summary>
        private UserManageModule UserManageModule{get;}

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="userManageModule">用户管理模块依赖</param>
        public RequireUserAttribute(UserManageModule userManageModule){
            this.UserManageModule=userManageModule;
        }

        /// <summary>
        /// 过滤
        /// </summary>
        /// <param name="context">上下文</param>
        public void OnAuthorization(AuthorizationFilterContext context) {
            if(context==null){return;}
            Response response=new Response();
            IHeaderDictionary headerDictionary=context.HttpContext.Request.Headers;
            if(!headerDictionary.ContainsKey("x-winds-username")){
                response.SetErrorType(1).SetErrorMessage("未提供 x-winds-username 头").SetReadableErrorMessage("鉴权失败");
                context.Result=new ContentResult{Content=response.Serialize(),ContentType="application/json",StatusCode=400};
                return;
            }
            if(!headerDictionary.ContainsKey("x-winds-password")){
                response.SetErrorType(1).SetErrorMessage("未提供 x-winds-password 头").SetReadableErrorMessage("鉴权失败");
                context.Result=new ContentResult{Content=response.Serialize(),ContentType="application/json",StatusCode=400};
                return;
            }
            String username=headerDictionary["x-winds-username"][0];
            String password=headerDictionary["x-winds-password"][0];
            if(String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password)){
                response.SetErrorType(1).SetErrorMessage("username 或 username 无效").SetReadableErrorMessage("鉴权失败");
                context.Result=new ContentResult{Content=response.Serialize(),ContentType="application/json",StatusCode=401};
                return;
            }
            (Boolean,User) authUser=this.UserManageModule.AuthUser(username,password);
            if(!authUser.Item1){
                response.SetErrorType(1).SetErrorMessage("检查用户失败").SetReadableErrorMessage("鉴权失败");
                context.Result=new ContentResult{Content=response.Serialize(),ContentType="application/json",StatusCode=401};
                return;
            }
            if(authUser.Item2==null){
                response.SetErrorType(1).SetErrorMessage("用户数据不存在").SetReadableErrorMessage("鉴权失败");
                context.Result=new ContentResult{Content=response.Serialize(),ContentType="application/json",StatusCode=401};
                return;
            }
            if(authUser.Item2.Disabled){
                response.SetErrorType(1).SetErrorMessage("用户已被禁用").SetReadableErrorMessage("鉴权失败");
                context.Result=new ContentResult{Content=response.Serialize(),ContentType="application/json",StatusCode=401};
                return;
            }
            context.HttpContext.Items["user"]=authUser.Item2;
        }
    }
}
