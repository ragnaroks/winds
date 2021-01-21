using HttpApi.FilterEntities.RequireManager;
using HttpApi.ModuleEntities.UserManage;
using HttpApi.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace HttpApi.Filters {
    /// <summary>管理员权限过滤器</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RequireManagerAttribute:Attribute, IAuthorizationFilter {
        /// <summary>用户管理模块依赖</summary>
        private UserManageModule UserManageModule{get;}

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="userManageModule">用户管理模块依赖</param>
        public RequireManagerAttribute(UserManageModule userManageModule){
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
            if(!this.UserManageModule.AuthUser(username,password,out User user)){
                response.SetErrorType(1).SetErrorMessage("检查用户失败").SetReadableErrorMessage("鉴权失败");
                context.Result=new ContentResult{Content=response.Serialize(),ContentType="application/json",StatusCode=401};
                return;
            }
            if(user==null){
                response.SetErrorType(1).SetErrorMessage("用户数据不存在").SetReadableErrorMessage("鉴权失败");
                context.Result=new ContentResult{Content=response.Serialize(),ContentType="application/json",StatusCode=401};
                return;
            }
            if(user.Disabled){
                response.SetErrorType(1).SetErrorMessage("用户已被禁用").SetReadableErrorMessage("鉴权失败");
                context.Result=new ContentResult{Content=response.Serialize(),ContentType="application/json",StatusCode=401};
                return;
            }
            if(user.Type>1){
                response.SetErrorType(1).SetErrorMessage("用户无权限访问").SetReadableErrorMessage("鉴权失败");
                context.Result=new ContentResult{Content=response.Serialize(),ContentType="application/json",StatusCode=403};
                return;
            }
            context.HttpContext.Items["user"]=user;
        }
    }
}
