using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HttpApi {
    /// <summary>应用程序环境配置</summary>
    public class Settings{
        /// <summary>Json序列号规则</summary>
        [JsonIgnore]
        public JsonSerializerOptions JsonSerializerOptions{get;}=new JsonSerializerOptions{PropertyNamingPolicy=null,IgnoreNullValues=false,PropertyNameCaseInsensitive=false,Encoder=System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping};
        /// <summary>物理根路径,有路径分隔符</summary>
        public String BaseDirectory{get;}=AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>日志根路径,无路径分隔符</summary>
        public String LogsDirectory{get;}=AppDomain.CurrentDomain.BaseDirectory+"logs";
        /// <summary>单元根路径,无路径分隔符</summary>
        public String UnitsDirectory{get;}=AppDomain.CurrentDomain.BaseDirectory+"units";
        /// <summary>日志根路径,无路径分隔符</summary>
        public String UsersDirectory{get;}=AppDomain.CurrentDomain.BaseDirectory+"users";
        /// <summary>是否开发模式</summary>
        public Boolean DevelopMode{get;private set;}
        /// <summary>数据根路径,无路径分隔符</summary>
        public String DataDirectory{get;private set;}=AppDomain.CurrentDomain.BaseDirectory+"data";
        /// <summary>允许跨域的origin</summary>
        public String[] AllowCorsOrigin{get;private set;}=new String[]{};
        
        /// <summary>
        /// 设置 DevelopMode
        /// </summary>
        /// <param name="developMode">是否开发模式</param>
        public void SetDevelopMode(Boolean developMode)=>this.DevelopMode=developMode;
        /// <summary>
        /// 设置 DataDirectory
        /// </summary>
        /// <param name="dataDirectory">数据根路径,无路径分隔符</param>
        public void SetDataDirectory(String dataDirectory) {
            if(String.IsNullOrWhiteSpace(dataDirectory)){return;}
            this.DataDirectory=dataDirectory;
        }
        /// <summary>
        /// 设置 AllowCorsOrigin
        /// </summary>
        /// <param name="allowCorsOrigin">允许跨域的origin</param>
        public void SetAllowCorsOrigin(String allowCorsOrigin) {
            if(String.IsNullOrWhiteSpace(allowCorsOrigin)){return;}
            String[] array=allowCorsOrigin.Split(';',StringSplitOptions.RemoveEmptyEntries);
            if(array==null || array.GetLength(0)<1){return;}
            this.AllowCorsOrigin=array;
        }

        /// <summary>
        /// 序列化
        /// </summary>
        public String Serialize()=>JsonSerializer.Serialize(this,this.JsonSerializerOptions);
    }
}
