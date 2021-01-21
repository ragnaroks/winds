using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace HttpApi.Helpers {
    public static class HttpRequestHelper {
        private static Regex RegexIPv4{get;}=new Regex(@"^[0-9\.]{7,15}$",RegexOptions.Compiled);
        private static Regex RegexIPv6{get;}=new Regex(@"^[a-f0-9\:]{3,39}$",RegexOptions.Compiled);

        /// <summary>
        /// 取 X-Real-Ip 头
        /// </summary>
        /// <param name="httpRequest"></param>
        public static String GetRealIp(HttpRequest httpRequest) {
            try {
                if(httpRequest==null){return "::";}
                IHeaderDictionary headers=httpRequest.Headers;
                if(!headers.ContainsKey("X-Real-IP")){return "::";}
                String xRealIP=headers["X-Real-IP"];
                if(String.IsNullOrWhiteSpace(xRealIP)){return "::";}
                if(RegexIPv6.IsMatch(xRealIP)){
                    return xRealIP;
                }else if(RegexIPv4.IsMatch(xRealIP)){
                    Boolean b1=IPAddress.TryParse(xRealIP,out IPAddress ipAddress);
                    if(!b1 || ipAddress==null) {
                        return "::ffff:"+xRealIP;
                    } else {
                        return ipAddress.MapToIPv6().ToString();
                    }
                }else{
                    return "::";
                }
            }catch{
                //这里可能会抛 HttpRequest 已经被释放的异常
                return "::";
            }
        }

        /// <summary>
        /// 取 X-Forwarded-For 头
        /// <para>X-Forwarded-For: client,proxy1,proxy2</para>
        /// </summary>
        /// <param name="httpRequest"></param>
        public static String GetForwardedFor(HttpRequest httpRequest) {
            try {
                if(httpRequest==null){return "::";}
                IHeaderDictionary headers=httpRequest.Headers;
                if(!headers.ContainsKey("X-Forwarded-For")){return "::";}
                String xForwardedFor=headers["X-Forwarded-For"];
                if(String.IsNullOrWhiteSpace(xForwardedFor)){return "::";}
                String[] xForwardedForArray=xForwardedFor.Replace(" ",String.Empty).Split(',',StringSplitOptions.RemoveEmptyEntries);//移除所有空格并按','来分组
                if(xForwardedForArray==null || xForwardedForArray.GetLength(0)<1){return "::";}
                String clientIp=xForwardedForArray[0];
                if(String.IsNullOrWhiteSpace(clientIp)){return "::";}
                if(RegexIPv6.IsMatch(clientIp)){
                    return clientIp;
                }else if(RegexIPv4.IsMatch(clientIp)){
                    Boolean b1=IPAddress.TryParse(clientIp,out IPAddress ipAddress);
                    if(!b1 || ipAddress==null) {
                        return "::ffff:"+clientIp;
                    } else {
                        return ipAddress.MapToIPv6().ToString();
                    }
                }else{
                    return "::";
                }
            } catch {
                //这里可能会抛 HttpRequest 已经被释放的异常
                return "::";
            }
        }

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <param name="httpRequest"></param>
        public static String GetClientIp(HttpRequest httpRequest) {
            String xRealIp=GetRealIp(httpRequest);
            if(xRealIp!="::"){return xRealIp;}
            String xForwaredFor=GetForwardedFor(httpRequest);
            if(xForwaredFor!="::"){return xForwaredFor;}
            return httpRequest.HttpContext.Connection.RemoteIpAddress.MapToIPv6().ToString();
        }
    }
}
