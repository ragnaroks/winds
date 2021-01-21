using System;

namespace HttpApi.ControllerEntities.Manage {
    public class AddUserRequest {
        public String Username{get;set;}
        public String Password{get;set;}
        public String Nickname{get;set;}
        public String Description{get;set;}
        public String Mark{get;set;}
        public Int32 Type{get;set;}
        public Boolean Enabled{get;set;}
    }
}
