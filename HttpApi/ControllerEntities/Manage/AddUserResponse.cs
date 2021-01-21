using System;

namespace HttpApi.ControllerEntities.Manage {
    public class AddUserResponse {
        public Int32 ErrorType{get;private set;}
        public String ErrorMessage{get;private set;}
        public String ReadableErrorMessage{get;private set;}
        public ModuleEntities.UserManage.User User{get;private set;}

        public AddUserResponse SetErrorType(Int32 errorType) {
            this.ErrorType=errorType;
            return this;
        }
        public AddUserResponse SetErrorMessage(String errorMessage) {
            this.ErrorMessage=errorMessage;
            return this;
        }
        public AddUserResponse SetReadableErrorMessage(String readableErrorMessage) {
            this.ReadableErrorMessage=readableErrorMessage;
            return this;
        }
        public AddUserResponse SetUser(ModuleEntities.UserManage.User user) {
            this.User=user;
            return this;
        }
    }
}
