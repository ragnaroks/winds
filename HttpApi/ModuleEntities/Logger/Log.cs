using HttpApi.ModuleEnums.Logger;
using System;

namespace HttpApi.ModuleEntities.Logger {
    public class Log {
        public Int64 Time{get;set;}
        public String Path{get;set;}
        public LogLevel LogLevel{get;set;}=LogLevel.Debug;
        public String Text{get;set;}
    }
}
