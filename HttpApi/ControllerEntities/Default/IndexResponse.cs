using System;

namespace HttpApi.ControllerEntities.Default {
    public class IndexResponse {
        public Int32 ErrorType{get;private set;}
        public String ErrorMessage{get;private set;}
        public String ReadableErrorMessage{get;private set;}
        public String Version{get;private set;}
        public String IpAddress{get;private set;}
        public String UserAgent{get;private set;}
        public String Origin{get;private set;}

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
        public IndexResponse SetVersion(String version) {
            this.Version=version;
            return this;
        }
        public IndexResponse SetIpAddress(String ipAddress) {
            this.IpAddress=ipAddress;
            return this;
        }
        public IndexResponse SetUserAgent(String userAgent) {
            this.UserAgent=userAgent;
            return this;
        }
        public IndexResponse SetOrigin(String origin) {
            this.Origin=origin;
            return this;
        }
    }
}
