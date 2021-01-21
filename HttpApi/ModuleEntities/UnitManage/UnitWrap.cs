using System;

namespace HttpApi.ModuleEntities.UnitManage {
    public class UnitWrap {
        /// <summary>单元名称</summary>
        public String Key{get;private set;}
        /// <summary>单元配置</summary>
        public UnitSettings UnitSettings{get;set;}=null;
        /// <summary>正在使用的配置</summary>
        public UnitSettings RunningUnitSettings{get;set;}=null;
        public UnitStatus UnitStatus{get;set;}=null;

        public void SetKey(String key)=>this.Key=key;
    }
}
