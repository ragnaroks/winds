using HttpApi.ControllerEntities.Manage;
using HttpApi.Filters;
using HttpApi.Modules;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HttpApi.Controllers {
    /// <summary>管理控制器</summary>
    [ServiceFilter(typeof(RequireManagerAttribute))]
    public class ManageController:Controller {
        /// <summary>
        /// 测试
        /// </summary>
        public JsonResult Index()=>Json(new IndexResponse());

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        public JsonResult AddUser([FromServices]UserManageModule userManageModule,[FromBody]AddUserRequest request) {
            AddUserResponse response=new AddUserResponse();
            //检查参数
            if(request==null) {
                response.SetErrorType(11).SetErrorMessage("参数无效").SetReadableErrorMessage("提交数据无效");
                return Json(response);
            }
            if(!userManageModule.UsernameValidate(request.Username)) {
                response.SetErrorType(11).SetErrorMessage("Username 参数无效").SetReadableErrorMessage("用户名无效");
                return Json(response);
            }
            if(String.IsNullOrWhiteSpace(request.Password)) {
                response.SetErrorType(11).SetErrorMessage("Password 参数无效").SetReadableErrorMessage("用户密码无效");
                return Json(response);
            }
            if(String.IsNullOrWhiteSpace(request.Nickname)) {
                response.SetErrorType(11).SetErrorMessage("Nickname 参数无效").SetReadableErrorMessage("用户昵称无效");
                return Json(response);
            }
            if(request.Type<1 || request.Type>2) {
                response.SetErrorType(11).SetErrorMessage("Type 参数无效").SetReadableErrorMessage("用户类型无效");
                return Json(response);
            }
            (Boolean,Boolean) userExist=userManageModule.UserExist(request.Username);
            if(!userExist.Item1) {
                response.SetErrorType(11).SetErrorMessage("UserExist 执行失败").SetReadableErrorMessage("检查用户是否已存在时发生错误");
                return Json(response);
            }
            if(userExist.Item2) {
                response.SetErrorType(11).SetErrorMessage("用户已存在").SetReadableErrorMessage("用户已存在");
                return Json(response);
            }
            //添加用户 12
            ModuleEntities.UserManage.User user=new ModuleEntities.UserManage.User{Password=request.Password,Nickname=request.Nickname,Description=request.Description,Mark=request.Mark,Type=request.Type,Disabled=request.Enabled};
            (Boolean,String) addUser=userManageModule.AddUser(request.Username,user);
            if(!addUser.Item1) {
                response.SetErrorType(12).SetErrorMessage("添加用户失败").SetReadableErrorMessage($"添加用户失败，{addUser.Item2}");
                return Json(response);
            }
            //
            response.SetUser(user);
            return Json(response);
        }
    }
}
