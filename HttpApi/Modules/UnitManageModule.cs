using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.IO;
using HttpApi.ModuleEntities.UnitManage;
using System.ComponentModel;
using HttpApi.ModuleOptions;

namespace HttpApi.Modules {
    [Localizable(false)]
    public class UnitManageModule {
        /// <summary>配置项</summary>
        private IOptions<UnitManageModuleOptions> Options{get;}
        /// <summary>日志模块</summary>
        private LoggerModule LoggerModule{get;}
        private ConcurrentDictionary<String,UnitWrap> UnitWrapDictionary{get;set;}=new ConcurrentDictionary<String,UnitWrap>();

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="options">配置项依赖</param>
        /// <param name="loggerModule">日志模块依赖</param>
        public UnitManageModule(IOptions<UnitManageModuleOptions> options,LoggerModule loggerModule){
            this.Options=options??throw new ArgumentNullException(nameof(options),"参数不能为空");
            this.LoggerModule=loggerModule??throw new ArgumentNullException(nameof(loggerModule),"参数不能为空");
            //检查目录
            if(String.IsNullOrWhiteSpace(this.Options.Value.UnitDirectory)){throw new Exception("单元存储目录无效");}
            if(!Directory.Exists(this.Options.Value.UnitDirectory)){
                try{
                    _=Directory.CreateDirectory(this.Options.Value.UnitDirectory);
                }catch{
                    throw;
                }
            }
            //
            loggerModule.Log(LogLevel.Warning,"Modules.UnitManageModule.UnitManageModule","单元管理模块已初始化");
        }

        public void LoadAllUnits(){}
        public void RemoveAllUnits(){}
        public void StartAllUnits(){}
        public void RestartAllUnits(){}
        public void StopAllUnits(){}
        public void LoadUnit(String key){}
        public void RemoveUnit(String key){}
        public void StartUnit(String key){}
        public void RestartUnit(String key){}
        public void StopUnit(String key){}
        public void AutoStartAllUnits(){}
        public void LogsUnit(String key){}
        public void AttachUnit(String key){}
        public void CommandUnit(String key){}

        
    }
}
