using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using HttpApi.ModuleEntities.UnitLogger;
using HttpApi.Options;

namespace HttpApi.Modules {
    /// <summary>单元日志模块</summary>
    public class UnitLoggerModule:IDisposable {
        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing) {
            //写完所有日志
            //this.TimerCallback(null);

            if (!disposedValue) {
                if (disposing) {
                    // TODO: 释放托管状态(托管对象)。
                    this.Timer?.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                this.UnitLogQueue=null;

                disposedValue=true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        ~UnitLoggerModule() {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose() {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }
        #endregion

        private ILogger<UnitLoggerModule> Logger{get;}
        private IOptions<UnitLoggerModuleOptions> Options{get;}
        /// <summary>定时器</summary>
        private Timer Timer{get;}=null;
        /// <summary>是否启用定时器</summary>
        private Boolean TimerEnable{get;set;}=false;
        /// <summary>日志</summary>
        private ConcurrentQueue<UnitLog> UnitLogQueue{get;set;}=null;

        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public UnitLoggerModule(ILogger<UnitLoggerModule> logger,IOptions<UnitLoggerModuleOptions> options){
            this.Logger=logger;
            this.Options=options;
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
            this.Logger.LogInformation("winds 单元日志模块已初始化");
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
