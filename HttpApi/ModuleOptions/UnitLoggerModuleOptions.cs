using System;

namespace HttpApi.ModuleOptions {
    public class UnitLoggerModuleOptions {
        /// <summary>单元日志存放目录</summary>
        public String UnitLogsDirectory{get;set;}
        /// <summary>写入间隔,单位毫秒</summary>
        public Int32 Period{get;set;}=1000;
    }
}
