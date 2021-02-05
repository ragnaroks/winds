using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using HttpApi.ModuleEntities.UnitLogger;
using HttpApi.ModuleOptions;

namespace HttpApi.Modules {
    /// <summary>单元日志模块</summary>
    public class UnitLoggerModule:IDisposable {
        /// <summary>选项</summary>
        private IOptions<UnitLoggerModuleOptions> Options{get;}
        /// <summary>日志</summary>
        private LoggerModule LoggerModule{get;}
        /// <summary>定时器</summary>
        private Timer Timer{get;}=null;
        /// <summary>是否启用定时器</summary>
        private Boolean TimerEnable{get;set;}=false;
        /// <summary>日志</summary>
        private ConcurrentQueue<UnitLog> UnitLogQueue{get;set;}=null;

        #region IDisposable
        private bool disposedValue;
        protected virtual void Dispose(bool disposing) {
            if(!disposedValue) {
                if(disposing) {
                    // TODO: 释放托管状态(托管对象)
                    this.Timer?.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue=true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~UnitLoggerModule()
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

        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public UnitLoggerModule(IOptions<UnitLoggerModuleOptions> options,LoggerModule loggerModule){
            this.Options=options??throw new ArgumentNullException(nameof(options),"参数不能为空");
            this.LoggerModule=loggerModule??throw new ArgumentNullException(nameof(loggerModule),"参数不能为空");
            //检查日志目录
            if(String.IsNullOrWhiteSpace(this.Options.Value.UnitLogsDirectory)){throw new Exception("单元日志存储目录无效");}
            if(!Directory.Exists(this.Options.Value.UnitLogsDirectory)){
                try{
                    _=Directory.CreateDirectory(this.Options.Value.UnitLogsDirectory);
                }catch{
                    throw;
                }
            }
            //初始化定时器
            try{
                this.Timer=new Timer(this.TimerCallback,null,0,this.Options.Value.Period<1000?1000:this.Options.Value.Period);
            }catch{
                throw;
            }
            //
            this.TimerEnable=true;
            this.UnitLogQueue=new ConcurrentQueue<UnitLog>();
            loggerModule.Log(LogLevel.Warning,"Modules.UnitLoggerModule.UnitLoggerModule","单元日志模块已初始化");
        }

        /// <summary>
        /// 定时器回调
        /// </summary>
        /// <param name="state"></param>
        private void TimerCallback(object state) {
            if(!this.TimerEnable || this.UnitLogQueue.Count<1){return;}
            this.TimerEnable=false;
            while(this.UnitLogQueue.Count>0) {
                if(!this.UnitLogQueue.TryDequeue(out UnitLog log) || log==null || String.IsNullOrWhiteSpace(log.Key) || String.IsNullOrWhiteSpace(log.Text)){
                    this.TimerEnable=true;
                    return;
                }
                String unitLogDirectory=String.Concat(this.Options.Value.UnitLogsDirectory,Path.DirectorySeparatorChar,log.Key);
                if(!Directory.Exists(unitLogDirectory)){
                    try{
                        _=Directory.CreateDirectory(unitLogDirectory);
                    }catch{
                        continue;
                    }
                }
                String unitLogPath=String.Concat(unitLogDirectory,Path.DirectorySeparatorChar,DateTimeOffset.FromUnixTimeSeconds(log.Time).ToString("yyyy-MM-dd",DateTimeFormatInfo.InvariantInfo),".log");
                try{
                    FileStream fs;
                    if(File.Exists(unitLogPath)) {
                        fs=File.Open(unitLogPath,FileMode.Append,FileAccess.Write,FileShare.Read);
                    }else{
                        fs=File.Open(unitLogPath,FileMode.Create,FileAccess.ReadWrite,FileShare.Read);
                        fs.Seek(0,SeekOrigin.End);
                    }
                    Byte[] buffer=Encoding.UTF8.GetBytes(log.Text);
                    fs.Write(buffer,0,buffer.GetLength(0));
                    fs.Dispose();
                }catch{
                    continue;
                }
            }
            this.TimerEnable=true;
        }

        /// <summary>
        /// 记录,收到的消息会丢失换行
        /// </summary>
        /// <param name="key"></param>
        /// <param name="text"></param>
        public void UnitLog(String key,String text){
            if(String.IsNullOrWhiteSpace(key) || String.IsNullOrWhiteSpace(text)){return;}
            UnitLog unitLog=new UnitLog{Time=DateTimeOffset.Now.ToUnixTimeSeconds(),Key=key,Text=text};
            this.UnitLogQueue.Enqueue(unitLog);
        }

        /*
        /// <summary>
        /// 读取日志的最后 N 行
        /// </summary>
        /// <param name="unitKey"></param>
        /// <param name="lineSize"></param>
        /// <returns></returns>
        public Boolean GetLogLastLines(String unitKey,Int32 lineSize,out String logFilePath,out String[] logLines){
            logFilePath=null;
            logLines=null;
            if(!this.Useable || String.IsNullOrWhiteSpace(unitKey)){return false;}
            String logDirectory=String.Concat(this.UnitLogsDirectory,Path.DirectorySeparatorChar,unitKey);
            if(!Directory.Exists(logDirectory)){return false;}
            String filename=String.Concat(DateTime.Now.ToString("yyyy-MM-dd",DateTimeFormatInfo.InvariantInfo),".log");
            logFilePath=String.Concat(logDirectory,Path.DirectorySeparatorChar,filename);
            if(!File.Exists(logFilePath)){return false;}
            String[] lines;
            try {
                lines=File.ReadLines(logFilePath,Encoding.UTF8).ToArray();
            }catch(Exception exception) {
                LoggerModuleHelper.TryLog(
                    "Modules.UnitLoggerModule.GetLogLastLines[Error]",
                    $"打开日志文件异常,:{exception.Message}\n异常堆栈:{exception.StackTrace}");
                return false;
            }
            if(lines==null || lines.GetLength(0)<1){return false;}
            if(lines.GetLength(0)<=lineSize){
                logLines=lines;
            } else {
                logLines=lines.AsSpan(lines.GetLength(0)-lineSize).ToArray();
            }
            return true;
        }*/

        public void WriteAllLogs(){}
    }
}
