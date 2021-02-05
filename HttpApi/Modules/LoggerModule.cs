using HttpApi.ModuleEntities.Logger;
using HttpApi.ModuleEnums.Logger;
using HttpApi.ModuleOptions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace HttpApi.Modules {
    /// <summary>日志模块/// </summary>
    [Localizable(false)]
    public class LoggerModule:IDisposable {
        /// <summary>配置项</summary>
        private IOptions<LoggerModuleOptions> Options{get;}
        /// <summary>日志</summary>
        private ConcurrentQueue<Log> LogQueue{get;}
        /// <summary>定时器</summary>
        private Timer Timer{get;}
        /// <summary>是否启用定时器</summary>
        private Boolean TimerEnable{get;set;}

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

        internal void Log(Microsoft.Extensions.Logging.LogLevel error,string v1,string v2) {
            throw new NotImplementedException();
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~LoggerModule()
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
        /// 构造
        /// </summary>
        /// <param name="options">配置项</param>
        public LoggerModule(IOptions<LoggerModuleOptions> options) {
            this.Options=options??throw new ArgumentNullException(nameof(options),"参数不能为空");
            if(String.IsNullOrWhiteSpace(options.Value.LogsDirectory)){throw new Exception("LogsDirectory 配置项无效");}
            if(!Directory.Exists(options.Value.LogsDirectory)){
                try{
                    _=Directory.CreateDirectory(options.Value.LogsDirectory);
                }catch{
                    throw;
                }
            }
            try{
                if(options.Value.DevelopMode){
                    this.Timer=new Timer(this.TimerCallbackDevelopMode,null,0,1000);
                }else{
                    this.Timer=new Timer(this.TimerCallbackProductMode,null,0,options.Value.WritePeriod<1000?1000:options.Value.WritePeriod);
                }
            }catch{
                throw;
            }
            this.TimerEnable=true;
        }

        private void TimerCallbackDevelopMode(object state){
            if(!this.TimerEnable){return;}
            this.TimerEnable=false;
            while(!this.LogQueue.IsEmpty) {
                if(!this.LogQueue.TryDequeue(out Log log) || log==null || String.IsNullOrWhiteSpace(log.Text)){
                    this.TimerEnable=true;
                    return;
                }
                String time=DateTime.Now.ToString("HH:mm:ss",DateTimeFormatInfo.InvariantInfo);
                ConsoleColor consoleColor=Console.ForegroundColor;
                switch(log.LogLevel){
                    case LogLevel.Debug:
                        Console.ForegroundColor=ConsoleColor.Magenta;
                        Console.Write($"[Debug] {log.Path} @{time}\n");
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor=ConsoleColor.Red;
                        Console.Write($"[Error] {log.Path} @{time}\n");
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor=ConsoleColor.Yellow;
                        Console.Write($"[Warning] {log.Path} @{time}\n");
                        break;
                    case LogLevel.Information:
                        Console.ForegroundColor=ConsoleColor.Cyan;
                        Console.Write($"[Information] {log.Path} @{time}\n");
                        break;
                }
                Console.ForegroundColor=consoleColor;
                Console.WriteLine("    "+log.Text);
            }
            this.TimerEnable=true;
        }

        private void TimerCallbackProductMode(object state) {
            if(!this.TimerEnable){return;}
            this.TimerEnable=false;
            String time=DateTime.Now.ToString("HH:mm:ss",DateTimeFormatInfo.InvariantInfo);
            String filename=DateTime.Now.ToString("yyyy-MM-dd",DateTimeFormatInfo.InvariantInfo);
            while(!this.LogQueue.IsEmpty) {
                if(!this.LogQueue.TryDequeue(out Log log) || log==null || String.IsNullOrWhiteSpace(log.Text)){
                    this.TimerEnable=true;
                    return;
                }
                String level=null;
                switch(log.LogLevel) {
                    case LogLevel.Debug:level="_debug";break;
                    case LogLevel.Error:level="_error";break;
                    case LogLevel.Warning:level="_warning";break;
                    default:break;
                }
                String directory=log.Path.Replace('.',Path.DirectorySeparatorChar);
                String logFileDirectory=$"{this.Options.Value.LogsDirectory}{Path.DirectorySeparatorChar}{directory}";
                if(!Directory.Exists(logFileDirectory)) {
                    try{
                        _=Directory.CreateDirectory(logFileDirectory);
                    }catch{
                        continue;
                    }
                }
                String logFilePath=$"{this.Options.Value.LogsDirectory}{Path.DirectorySeparatorChar}{directory}{Path.DirectorySeparatorChar}{filename}{level}.log";
                FileStream fs=null;
                try{
                    if(File.Exists(logFilePath)) {
                        fs=new FileStream(logFilePath,FileMode.Append,FileAccess.Write,FileShare.Read);
                    }else{
                        fs=new FileStream(logFilePath,FileMode.Create,FileAccess.ReadWrite,FileShare.Read);
                        //fs.Seek(0,SeekOrigin.End);
                    }
                    Byte[] buffer=Encoding.UTF8.GetBytes($"{log.Path} @{time}\n{log.Text}\n");
                    fs.Write(buffer,0,buffer.GetLength(0));
                }catch{
                    continue;
                }finally{
                    fs?.Dispose();
                }
            }
            this.TimerEnable=true;
        }

        /// <summary>
        /// 记录
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        public void Log(LogLevel logLevel,String path,String text){
            if(logLevel<this.Options.Value.LogLevel || String.IsNullOrWhiteSpace(path) || String.IsNullOrWhiteSpace(text)){return;}
            Regex regex=new Regex(@"^[a-zA-Z0-9\.\[\]]{1,255}$",RegexOptions.Compiled);
            if(!regex.IsMatch(path)){return;}
            if(path[0]=='.' || path[^1]=='.'){return;}
            Log log=new Log{LogLevel=logLevel,Time=DateTimeOffset.Now.ToUnixTimeSeconds(),Path=path,Text=text};
            this.LogQueue.Enqueue(log);
        }
    }
}
