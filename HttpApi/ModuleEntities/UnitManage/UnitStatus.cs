using System;

namespace HttpApi.ModuleEntities.UnitManage {
    public class UnitStatus {
        /// <summary>单元</summary>
        public String Key{get;private set;}
        /// <summary>单元状态,0:未定义,1:已停止,2:正在启动,3:运行中,4:正在停止</summary>
        public Int32 State{get;set;}=0;
        /// <summary>进程id</summary>
        public Int32 ProcessId{get;set;}=0;
        /// <summary>内存使用量,单位字节</summary>
        public Int32 MemoryUsage{get;set;}=0;
        /// <summary>处理器使用量,扩大100倍的值,1248=>12.48%</summary>
        public Int32 ProcessorUsage{get;set;}=0;
        /// <summary>处理器综合使用量,扩大100倍的值,312=>3.12%</summary>
        public Int32 ProcessorsUsage{get;set;}=0;

        public void SetKey(String key)=>this.Key=key;
    }
}
