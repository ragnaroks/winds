using HttpApi.ModuleEntities.UserManage;
using HttpApi.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HttpApi.Modules {
    public class UserManageModule:IDisposable {
        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing) {
            if(!disposedValue) {
                if(disposing) {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                this.UserDictionary=null;

                disposedValue=true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~UserManageModule()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>日志</summary>
        private ILogger<UserManageModule> Logger{get;}
        /// <summary>选项</summary>
        private IOptions<UserManageModuleOptions> Options{get;}
        /// <summary>用户字典</summary>
        private ConcurrentDictionary<String,User> UserDictionary{get;set;}=new ConcurrentDictionary<String,User>();
        /// <summary>用户名格式正则</summary>
        private Regex UsernameRegex{get;}=new Regex(@"^[a-z0-9]{3,16}$",RegexOptions.Compiled);

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="logger">日志</param>
        /// <param name="options">选项</param>
        public UserManageModule(ILogger<UserManageModule> logger,IOptions<UserManageModuleOptions> options) {
            this.Logger=logger;
            this.Options=options;
            logger.LogInformation("用户管理模块初始化开始");
            if(String.IsNullOrWhiteSpace(options.Value.UsersDirectory) || !Directory.Exists(options.Value.UsersDirectory)){throw new ArgumentNullException("用户管理模块初始化异常","UsersDirectory");}
            this.LoadAllUsers(out Int32 count);
            logger.LogInformation($"用户管理模块初始化完成，加载 {count} 个用户");
        }

        /// <summary>
        /// 解析用户配置文件
        /// </summary>
        /// <param name="filePath">用户配置文件路径</param>
        /// <param name="user">用户实体</param>
        private Boolean Parse(String filePath,out User user){
            user=null;
            if(String.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)){return false;}
            FileStream fileStream;
            try{
                fileStream=File.Open(filePath,FileMode.Open,FileAccess.Read,FileShare.Read);
            }catch(Exception exception){
                this.Logger.LogError(exception,"解析用户配置文件失败，打开文件异常");
                return false;
            }
            Byte[] bytes=new Byte[fileStream.Length];
            try {
                fileStream.Read(bytes,0,bytes.GetLength(0));
            }catch(Exception exception){
                this.Logger.LogError(exception,"解析用户配置文件失败，读取文件异常");
                return false;
            }finally{
                fileStream.Dispose();
            }
            try {
                user=JsonSerializer.Deserialize<User>(bytes);
            }catch(Exception exception){
                this.Logger.LogError(exception,"解析用户配置文件失败，反序列化异常");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 加载所有用户配置
        /// </summary>
        /// <returns>加载成功的用户配置数量</returns>
        private Boolean LoadAllUsers(out Int32 loadedCount){
            loadedCount=0;
            DirectoryInfo directoryInfo;
            FileInfo[] fileInfoArray;
            try{
                directoryInfo=new DirectoryInfo(this.Options.Value.UsersDirectory);
                fileInfoArray=directoryInfo.GetFiles("*.json",SearchOption.TopDirectoryOnly);
            }catch(Exception exception){
                this.Logger.LogError(exception,"加载所有用户配置时，访问用户配置根目录异常");
                return false;
            }
            if(fileInfoArray.GetLength(0)<1){
                this.Logger.LogWarning("加载所有用户配置时，没有找到任何用户配置文件");
                return false;
            }
            for(Int32 i1=0;i1<fileInfoArray.GetLength(0);i1++){
                Int32 index=fileInfoArray[i1].Name.IndexOf('.');
                if(index<0){continue;}
                String username=fileInfoArray[i1].Name.Remove(index);
                this.UsernameValidate(username,out Boolean validate);
                if(!validate){continue;}
                if(!this.Parse(fileInfoArray[i1].FullName,out User user)){
                    this.Logger.LogWarning("加载所有用户配置时，解析用户配置文件失败");
                    continue;
                }
                if(!this.UserDictionary.TryAdd(username,user)) {
                    this.Logger.LogWarning("加载所有用户配置时，添加到用户配置字典失败");
                    continue;
                }
                loadedCount++;
            }
            return true;
        }

        /// <summary>
        /// 添加用户配置文件
        /// </summary>
        /// <param name="user">用户实体</param>
        /// <param name="errorMessage">错误消息</param>
        public Boolean AddUser(String username,User user,out String errorMessage) {
            errorMessage=null;
            this.UsernameValidate(username,out Boolean validate);
            if(!validate || String.IsNullOrWhiteSpace(user.Password) || this.UserDictionary.ContainsKey(username)){
                errorMessage="用户名检查失败";
                return false;
            }
            String filePath=String.Concat(Program.Settings.UsersDirectory,Path.DirectorySeparatorChar,username,".json");
            if(File.Exists(filePath)){
                errorMessage="用户配置文件已存在";
                return false;
            }
            FileStream fileStream;
            try {
                fileStream=File.Open(filePath,FileMode.Create,FileAccess.Write,FileShare.Read);
            }catch(Exception exception){
                this.Logger.LogError(exception,"添加用户配置文件时，创建文件异常");
                errorMessage="创建文件异常";
                return false;
            }
            Byte[] fileData=JsonSerializer.SerializeToUtf8Bytes(user);
            try {
                fileStream.Write(fileData,0,fileData.GetLength(0));
                fileStream.Flush();
            }catch(Exception exception) {
                this.Logger.LogError(exception,"添加用户配置文件时，写入文件异常");
                errorMessage="写入文件异常";
                return false;
            }finally{
                fileStream.Dispose();
            }
            if(!this.UserDictionary.TryAdd(username,user)){
                errorMessage="添加到用户字典失败";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 删除用户配置文件
        /// </summary>
        /// <param name="username"></param>
        /// <returns>是否成功</returns>
        public Boolean RemoveUser(String username){
            this.UsernameValidate(username,out Boolean validate);
            if(!validate || !this.UserDictionary.ContainsKey(username)){return false;}
            String filePath=String.Concat(Program.Settings.UsersDirectory,Path.DirectorySeparatorChar,username,".json");
            if(!File.Exists(filePath)){return false;}
            try{
                File.Delete(filePath);
            }catch(Exception exception){
                this.Logger.LogError(exception,"删除用户配置文件时异常");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 修改用户配置文件
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="modifiedUser">用户实体</param>
        public Boolean ModifyUser(String username,User modifiedUser) {
            this.UsernameValidate(username,out Boolean validate);
            if(!validate || !this.UserDictionary.ContainsKey(username)){return false;}
            String filePath=String.Concat(Program.Settings.UsersDirectory,Path.DirectorySeparatorChar,username,".json");
            if(!File.Exists(filePath)){return false;}
            FileStream fileStream;
            try {
                fileStream=File.Open(filePath,FileMode.Truncate,FileAccess.Write,FileShare.Read);
            }catch(Exception exception){
                this.Logger.LogError(exception,"修改用户配置文件时，打开文件异常");
                return false;
            }
            Byte[] fileData=JsonSerializer.SerializeToUtf8Bytes(modifiedUser);
            try {
                fileStream.Write(fileData,0,fileData.GetLength(0));
                fileStream.Flush();
            }catch(Exception exception) {
                this.Logger.LogError(exception,"修改用户配置文件时，写入文件异常");
                return false;
            }finally{
                fileStream.Dispose();
            }
            return true;
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="user">用户实体</param>
        public Boolean GetUser(String username,out User user){
            this.UsernameValidate(username,out Boolean validate);
            if(!validate || this.UserDictionary.Count<1){
                user=null;
                return false;
            }
            return this.UserDictionary.TryGetValue(username,out user);
        }

        /// <summary>
        /// 验证用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="user">用户实体</param>
        public Boolean AuthUser(String username,String password,out User user){
            this.UsernameValidate(username,out Boolean validate);
            if(!validate || String.IsNullOrWhiteSpace(password) || this.UserDictionary.Count<1){
                user=null;
                return false;
            }
            return this.UserDictionary.TryGetValue(username,out user) && user.Password==password;
        }

        /// <summary>
        /// 检查用户名格式
        /// </summary>
        /// <param name="username"></param>
        /// <param name="validate">是否有效</param>
        public void UsernameValidate(String username,out Boolean validate) {
            validate=!String.IsNullOrWhiteSpace(username) && this.UsernameRegex.IsMatch(username);
        }

        /// <summary>
        /// 用户名是否已存在
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="exist">用户名是否已存在</param>
        public Boolean UserExist(String username,out Boolean exist) {
            this.UsernameValidate(username,out Boolean validate);
            if(!validate){
                exist=false;
                return false;
            }
            exist=this.UserDictionary.Count>0 && this.UserDictionary.ContainsKey(username);
            return true;
        }
    }
}
