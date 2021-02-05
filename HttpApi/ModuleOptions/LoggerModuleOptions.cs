using HttpApi.ModuleEnums.Logger;
using System;

namespace HttpApi.ModuleOptions {
    public class LoggerModuleOptions {
        /// <summary>日志目录</summary>
        public String LogsDirectory{get;set;}
        /// <summary>文件写入间隔</summary>
        public Int32 WritePeriod{get;set;}=1000;
        /// <summary>是否开发模式</summary>
        public Boolean DevelopMode{get;set;}
        /// <summary>LogLevel</summary>
        public LogLevel LogLevel{get;set;}
    }
}
