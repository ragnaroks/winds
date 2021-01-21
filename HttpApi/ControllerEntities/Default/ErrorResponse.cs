using System;

namespace HttpApi.ControllerEntities.Default {
    public class ErrorResponse {
        public Int32 ErrorType{get;private set;}
        public String ErrorMessage{get;private set;}
        public String ReadableErrorMessage{get;private set;}
        public String ActivityCurrentId{get;private set;}
        public String TraceIdentifier{get;private set;}

        public ErrorResponse SetErrorType(Int32 errorType) {
            this.ErrorType=errorType;
            return this;
        }
        public ErrorResponse SetErrorMessage(String errorMessage) {
            this.ErrorMessage=errorMessage;
            return this;
        }
        public ErrorResponse SetReadableErrorMessage(String readableErrorMessage) {
            this.ReadableErrorMessage=readableErrorMessage;
            return this;
        }
        public ErrorResponse SetActivityCurrentId(String activityCurrentId) {
            this.ActivityCurrentId=activityCurrentId;
            return this;
        }
        public ErrorResponse SetTraceIdentifier(String traceIdentifier) {
            this.TraceIdentifier=traceIdentifier;
            return this;
        }
    }
}
