using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;

namespace HttpApi {
    public static class Program {
        /// <summary>应用程序配置</summary>
        public static Settings Settings{get;}=new Settings();
        /// <summary>互斥</summary>
        public static Mutex Mutex{get;private set;}

        public static void Main(String[] args) {
            Program.Mutex=new Mutex(true,"WIND_DAEMON_MUTEX",out Boolean mutex);
            if(!mutex || Program.Mutex==null){
                Environment.Exit(0);
                return;
            }
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder=>{
                    webBuilder.UseStartup<Startup>();
                })
                .Build()
                .Run();
            Program.Mutex.ReleaseMutex();
            Program.Mutex.Dispose();
        }
    }
}
