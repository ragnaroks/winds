using System;
using System.Collections.Generic;

namespace HttpApi.ModuleEntities.UnitManage {
    public class UnitSettings {
        /// <summary>单元</summary>
        public String Key{get;private set;}
        /// <summary>名称</summary>
        public String Name{get;set;}
        /// <summary>描述</summary>
        public String Description{get;set;}
        /// <summary>工作目录</summary>
        public String WorkDirectory{get;set;}
        /// <summary>启动指令</summary>
        public String StartCommand{get;set;}
        /// <summary>启动指令参数</summary>
        public String StartCommandArguments{get;set;}
        /// <summary>停止指令</summary>
        public String StopCommand{get;set;}
        /// <summary>停止指令参数</summary>
        public String StopCommandArguments{get;set;}
        /// <summary>重启指令</summary>
        public String RestartCommand{get;set;}
        /// <summary>重启指令参数</summary>
        public String RestartCommandArguments{get;set;}
        /// <summary>单元是否自启,默认不自启</summary>
        public Boolean AutoStart{get;set;}=false;
        /// <summary>单元自启延迟(秒),默认10秒</summary>
        public Int32 AutoStartDelay{get;set;}=10;
        /// <summary>守护进程</summary>
        public Boolean RestartWhenException{get;set;}=false;
        /// <summary>进程优先级</summary>
        public String PriorityClass{get;set;}=null;
        /// <summary>CPU亲和性,"0,1,3,5"</summary>
        public String ProcessorAffinity{get;set;}=null;
        /// <summary>环境变量</summary>
        public Dictionary<String,String> EnvironmentVariables{get;set;}=null;
        /// <summary>是否获取性能使用数据</summary>
        public Boolean MonitorPerformanceUsage{get;set;}=false;
        /// <summary>是否获取网络使用数据</summary>
        public Boolean MonitorNetworkUsage{get;set;}=false;


        public void SetKey(String key)=>this.Key=key;
    }
}
