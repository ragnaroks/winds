using System;
using System.Text.Json;

namespace HttpApi.FilterEntities.RequireUser {
    public class Response {
        public Int32 ErrorType{get;private set;}
        public String ErrorMessage{get;private set;}
        public String ReadableErrorMessage{get;private set;}

        public Response SetErrorType(Int32 errorType) {
            this.ErrorType=errorType;
            return this;
        }
        public Response SetErrorMessage(String errorMessage) {
            this.ErrorMessage=errorMessage;
            return this;
        }
        public Response SetReadableErrorMessage(String readableErrorMessage) {
            this.ReadableErrorMessage=readableErrorMessage;
            return this;
        }

        public String Serialize()=>JsonSerializer.Serialize(this);
    }
}
