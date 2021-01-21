using System;
using System.Collections.Generic;

namespace HttpApi.ModuleEntities.UserManage {
    public class User {
        public String Password{get;set;}
        public String Nickname{get;set;}
        public String Description{get;set;}
        public String Mark{get;set;}
        public Int32 Type{get;set;}
        public Boolean Disabled{get;set;}
        public List<UserRunAsPartial> RunAsList{get;set;}
        public List<String> UnitList{get;set;}
    }
}
