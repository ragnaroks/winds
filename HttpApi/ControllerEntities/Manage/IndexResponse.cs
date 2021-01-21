using System;

namespace HttpApi.ControllerEntities.Manage {
    public class IndexResponse {
        public Int32 ErrorType{get;private set;}
        public String ErrorMessage{get;private set;}
        public String ReadableErrorMessage{get;private set;}

        public IndexResponse SetErrorType(Int32 errorType) {
            this.ErrorType=errorType;
            return this;
        }
        public IndexResponse SetErrorMessage(String errorMessage) {
            this.ErrorMessage=errorMessage;
            return this;
        }
        public IndexResponse SetReadableErrorMessage(String readableErrorMessage) {
            this.ReadableErrorMessage=readableErrorMessage;
            return this;
        }
    }
}
