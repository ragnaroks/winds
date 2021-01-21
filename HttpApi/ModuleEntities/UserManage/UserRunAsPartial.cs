using System;
using System.Security;

namespace HttpApi.ModuleEntities.UserManage {
    public class UserRunAsPartial {
        public String Username{get;set;}
        public String Password{get;set;}

        /// <summary>
        /// 获取密码的安全字符串，别忘了释放
        /// </summary>
        public SecureString GetPasswordSecureString(){
            if(String.IsNullOrWhiteSpace(this.Password)){return null;}
            SecureString secureString=new SecureString();
            for(Int32 i1=0;i1<this.Password.Length;i1++){ secureString.AppendChar(this.Password[i1]); }
            secureString.MakeReadOnly();
            return secureString;
        }
    }
}
