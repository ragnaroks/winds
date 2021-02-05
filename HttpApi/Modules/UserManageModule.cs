using HttpApi.ModuleEntities.UserManage;
using HttpApi.ModuleEnums.Logger;
using HttpApi.ModuleOptions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HttpApi.Modules {
    [Localizable(false)]
    public class UserManageModule {
        /// <summary>选项</summary>
        private IOptions<UserManageModuleOptions> Options{get;}
        /// <summary>日志</summary>
        private LoggerModule LoggerModule{get;}
        /// <summary>用户名格式正则</summary>
        private Regex UsernameRegex{get;}=new Regex(@"^[a-z0-9]{3,16}$",RegexOptions.Compiled);
        /// <summary>用户字典</summary>
        private ConcurrentDictionary<String,User> UserDictionary{get;set;}=new ConcurrentDictionary<String,User>();
        
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="options">配置项依赖</param>
        /// <param name="loggerModule">日志模块依赖</param>
        public UserManageModule(IOptions<UserManageModuleOptions> options,LoggerModule loggerModule) {
            this.Options=options??throw new ArgumentNullException(nameof(options),"参数不能为空");
            this.LoggerModule=loggerModule??throw new ArgumentNullException(nameof(loggerModule),"参数不能为空");
            loggerModule.Log(LogLevel.Warning,"Modules.UserManageModule.UserManageModule","用户管理模块初始化开始");
            if(String.IsNullOrWhiteSpace(options.Value.UsersDirectory) || !Directory.Exists(options.Value.UsersDirectory)){throw new ArgumentNullException("用户管理模块初始化异常","UsersDirectory");}
            (Boolean success,Int32 count)=this.LoadAllUsers();
            if(success){
                loggerModule.Log(LogLevel.Warning,"Modules.UserManageModule.UserManageModule",$"加载 {count} 个用户");
            }else{
                loggerModule.Log(LogLevel.Warning,"Modules.UserManageModule.UserManageModule","加载用户失败");
            }
            loggerModule.Log(LogLevel.Warning,"Modules.UserManageModule.UserManageModule","用户管理模块初始化完成");
        }

        /// <summary>
        /// 解析用户配置文件
        /// </summary>
        /// <param name="filePath">用户配置文件路径</param>
        /// <returns>(方法是否执行成功，用户实体)</returns>
        private (Boolean,User) Parse(String filePath){
            if(String.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)){return (false,null);}
            FileStream fileStream;
            try{
                fileStream=File.Open(filePath,FileMode.Open,FileAccess.Read,FileShare.Read);
            }catch(Exception exception){
                this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.Parse",$"解析用户配置文件失败，打开文件异常，{exception.Message}，{exception.StackTrace}");
                return (false,null);
            }
            Memory<Byte> bytes=new Memory<Byte>();
            try {
                fileStream.Read(bytes.Span);
            }catch(Exception exception){
                this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.Parse",$"解析用户配置文件失败，读取文件异常，{exception.Message}，{exception.StackTrace}");
                return (false,null);
            }finally{
                fileStream.Dispose();
            }
            User user;
            try {
                user=JsonSerializer.Deserialize<User>(bytes.Span);
            }catch(Exception exception){
                this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.Parse",$"解析用户配置文件失败，反序列化异常，{exception.Message}，{exception.StackTrace}");
                return (false,null);
            }
            return (true,user);
        }

        /// <summary>
        /// 检查用户名格式
        /// </summary>
        /// <param name="username"></param>
        /// <returns>是否有效</returns>
        public Boolean UsernameValidate(String username)=>!String.IsNullOrWhiteSpace(username) && this.UsernameRegex.IsMatch(username);

        /// <summary>
        /// 计数
        /// </summary>
        /// <returns></returns>
        public Int32 Count()=>this.UserDictionary.Count;

        /// <summary>
        /// 加载所有用户配置
        /// </summary>
        /// <returns>(方法是否执行成功，加载成功的用户配置数量)</returns>
        private (Boolean success,Int32 count) LoadAllUsers(){
            DirectoryInfo directoryInfo;
            FileInfo[] fileInfoArray;
            try{
                directoryInfo=new DirectoryInfo(this.Options.Value.UsersDirectory);
                fileInfoArray=directoryInfo.GetFiles("*.json",SearchOption.TopDirectoryOnly);
            }catch(Exception exception){
                this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.LoadAllUsers",$"加载所有用户配置时，访问用户配置根目录异常，{exception.Message}，{exception.StackTrace}");
                return (false,0);
            }
            if(fileInfoArray.GetLength(0)<1){
                this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.LoadAllUsers","加载所有用户配置时，没有找到任何用户配置文件");
                return (false,0);
            }
            Int32 count=0;
            for(Int32 i1=0;i1<fileInfoArray.GetLength(0);i1++){
                Int32 index=fileInfoArray[i1].Name.IndexOf('.');
                if(index<0){continue;}
                String username=fileInfoArray[i1].Name.Remove(index);
                if(!this.UsernameValidate(username)){continue;}
                (Boolean success,User user)=this.Parse(fileInfoArray[i1].FullName);
                if(!success){
                    this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.LoadAllUsers","加载所有用户配置时，解析用户配置文件失败");
                    continue;
                }
                if(!this.UserDictionary.TryAdd(username,user)) {
                    this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.LoadAllUsers","加载所有用户配置时，添加到用户配置字典失败");
                    continue;
                }
                count++;
            }
            return (true,count);
        }

        /// <summary>
        /// 添加用户配置文件
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="user">用户实体</param>
        /// <returns>(方法是否执行成功，错误消息)</returns>
        public (Boolean,String) AddUser(String username,User user) {
            if(!this.UsernameValidate(username) || String.IsNullOrWhiteSpace(user.Password) || this.UserDictionary.ContainsKey(username)){return (false,"用户名检查失败");}
            String filePath=String.Concat(Program.Settings.UsersDirectory,Path.DirectorySeparatorChar,username,".json");
            if(File.Exists(filePath)){return (false,"用户配置文件已存在");}
            FileStream fileStream;
            try {
                fileStream=File.Open(filePath,FileMode.Create,FileAccess.Write,FileShare.Read);
            }catch(Exception exception){
                this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.AddUser",$"添加用户配置文件时，创建文件异常，{exception.Message}，{exception.StackTrace}");
                return (false,"创建文件异常");
            }
            Byte[] fileData=JsonSerializer.SerializeToUtf8Bytes(user);
            try {
                fileStream.Write(fileData,0,fileData.GetLength(0));
                fileStream.Flush();
            }catch(Exception exception) {
                this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.AddUser",$"添加用户配置文件时，写入文件异常，{exception.Message}，{exception.StackTrace}");
                return (false,"写入文件异常");
            }finally{
                fileStream.Dispose();
            }
            if(!this.UserDictionary.TryAdd(username,user)){
                return (false,"添加到用户字典失败");
            }
            return (true,null);
        }

        /// <summary>
        /// 删除用户配置文件
        /// </summary>
        /// <param name="username"></param>
        /// <returns>方法是否执行成功</returns>
        public Boolean RemoveUser(String username){
            if(!this.UsernameValidate(username) || !this.UserDictionary.ContainsKey(username)){return false;}
            String filePath=String.Concat(Program.Settings.UsersDirectory,Path.DirectorySeparatorChar,username,".json");
            if(!File.Exists(filePath)){return false;}
            try{
                File.Delete(filePath);
            }catch(Exception exception){
                this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.RemoveUser",$"删除用户配置文件时异常，{exception.Message}，{exception.StackTrace}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 修改用户配置文件
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="modifiedUser">用户实体</param>
        /// <returns>方法是否执行成功</returns>
        public Boolean ModifyUser(String username,User modifiedUser) {
            if(!this.UsernameValidate(username) || !this.UserDictionary.ContainsKey(username)){return false;}
            String filePath=String.Concat(Program.Settings.UsersDirectory,Path.DirectorySeparatorChar,username,".json");
            if(!File.Exists(filePath)){return false;}
            FileStream fileStream;
            try {
                fileStream=new FileStream(filePath,FileMode.Truncate,FileAccess.Write,FileShare.Read);
            }catch(Exception exception){
                this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.RemoveUser",$"修改用户配置文件时打开文件异常，{exception.Message}，{exception.StackTrace}");
                return false;
            }
            Memory<Byte> bytes=JsonSerializer.SerializeToUtf8Bytes(modifiedUser);
            try {
                fileStream.Write(bytes.Span);
                fileStream.Flush();
            }catch(Exception exception) {
                this.LoggerModule.Log(LogLevel.Error,"Modules.UserManageModule.RemoveUser",$"修改用户配置文件时写入文件异常，{exception.Message}，{exception.StackTrace}");
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
        /// <returns>(方法是否执行成功，用户实体)</returns>
        public (Boolean,User) GetUser(String username){
            if(!this.UsernameValidate(username) || this.UserDictionary.Count<1){return (false,null);}
            return (this.UserDictionary.TryGetValue(username,out User user),user);
        }

        /// <summary>
        /// 验证用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>(方法是否执行成功，用户实体)</returns>
        public (Boolean,User) AuthUser(String username,String password){
            if(!this.UsernameValidate(username) || String.IsNullOrWhiteSpace(password) || this.UserDictionary.Count<1){return (false,null);}
            return (this.UserDictionary.TryGetValue(username,out User user) && user.Password==password,user);
        }

        /// <summary>
        /// 用户名是否已存在
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>(方法是否执行成功，用户名是否已存在)</returns>
        public (Boolean,Boolean) UserExist(String username){
            if(!this.UsernameValidate(username)){return (false,false);}
            return (true,this.UserDictionary.Count>0 && this.UserDictionary.ContainsKey(username));
        }
    }
}
