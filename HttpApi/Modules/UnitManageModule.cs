using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.IO;
using HttpApi.ModuleEntities.UnitManage;
using HttpApi.Options;

namespace HttpApi.Modules {
    public class UnitManageModule:IDisposable {
        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing) {
            if(!disposedValue) {
                if(disposing) {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                this.UnitWrapDictionary=null;

                disposedValue=true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~UnitHostModule()
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

        private ILogger<UnitManageModule> Logger{get;}
        private IOptions<UnitManageModuleOptions> Options{get;}
        private ConcurrentDictionary<String,UnitWrap> UnitWrapDictionary{get;set;}=new ConcurrentDictionary<String,UnitWrap>();

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public UnitManageModule(ILogger<UnitManageModule> logger,IOptions<UnitManageModuleOptions> options){
            this.Logger=logger;
            this.Options=options;
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
            this.Logger.LogInformation("winds 单元管理模块已初始化");
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
