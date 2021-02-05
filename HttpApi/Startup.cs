using HttpApi.ModuleEnums.Logger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace HttpApi {
    public class Startup {
        //public IConfiguration Configuration{get;}

        public Startup(IConfiguration configuration) {
            //this.Configuration=configuration;
            Program.Settings.SetAllowCorsOrigin(configuration.GetValue<String>("AllowCorsOrigin",null));
            Program.Settings.SetDataDirectory(configuration.GetValue<String>("DataDirectory",null));
            //验证
            if(String.IsNullOrWhiteSpace(Program.Settings.DataDirectory) || !Directory.Exists(Program.Settings.DataDirectory)){throw new ArgumentNullException("DataDirectory","数据根目录无效");}
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services){
            //mvcbuilder
            services.AddControllers().AddJsonOptions(options=>{
                options.JsonSerializerOptions.PropertyNamingPolicy=null;//保持大小写
                options.JsonSerializerOptions.IgnoreNullValues=false;//不忽略空值
                options.JsonSerializerOptions.PropertyNameCaseInsensitive=false;//true:大小写不敏感,false:大小写敏感,
                options.JsonSerializerOptions.Encoder=System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            });
            //servicecollection
            services.AddCors(options=>{
                options.AddDefaultPolicy(builder=>{
                    builder.WithOrigins(Program.Settings.AllowCorsOrigin).AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetPreflightMaxAge(new TimeSpan(1,0,0,0));
                });
            });
            //日志模块
            services.AddSingleton<Modules.LoggerModule>().Configure<ModuleOptions.LoggerModuleOptions>(options=>{
                options.LogsDirectory=Program.Settings.LogsDirectory;
                options.WritePeriod=1000;
                options.DevelopMode=Program.Settings.DevelopMode;
            });
            services.AddSingleton<Modules.UserManageModule>().Configure<ModuleOptions.UserManageModuleOptions>(options=>{
                options.UsersDirectory=Program.Settings.UsersDirectory;
            });
            services.AddSingleton<Filters.RequireUserAttribute>();
            services.AddSingleton<Filters.RequireManagerAttribute>();
            /*services.AddSingleton<Modules.UnitManageModule>().Configure<Options.UnitManageModuleOptions>(options=>{
                options.UnitDirectory=Program.Settings.UnitsDirectory;
            });
            services.AddSingleton<Modules.UnitLoggerModule>().Configure<Options.UnitLoggerModuleOptions>(options=>{
                options.UnitLogsDirectory=Program.Settings.LogsDirectory;
                options.Period=1000;
            })*/;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,IWebHostEnvironment env,IHostApplicationLifetime lifetime,
            Filters.RequireUserAttribute requireUserAttribute,Filters.RequireManagerAttribute requireManageAttributer,
            Modules.LoggerModule loggerModule,Modules.UserManageModule userManageModule){
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                Program.Settings.SetDevelopMode(true);
            } else {
                app.UseExceptionHandler("/Default/Error");
                app.UseMiddleware<Middlewares.ErrorHandlingMiddleware>();
            }
            app.UseCors();
            app.UseRouting();
            app.UseEndpoints(endpoints=>{
                endpoints.MapControllerRoute(name:"default",pattern:"{controller=Default}/{action=Index}/{id?}");
            });

            //生命周期
            lifetime.ApplicationStarted.Register(()=>{
                loggerModule.Log(LogLevel.Warning,"Startup.Configure","winds 服务主机已启动");
                //过滤器预热
                _=requireUserAttribute.GetHashCode();
                _=requireManageAttributer.GetHashCode();
                //模块预热
                _=userManageModule.GetHashCode();
                //_=unitManageModule.GetHashCode();
                //_=unitLoggerModule.GetHashCode();
            });
            lifetime.ApplicationStopping.Register(()=>{
                loggerModule.Log(LogLevel.Warning,"Startup.Configure","winds 服务主机正在停止");
                // 停止所有单元
                //unitManageModule.StopAllUnits();
                // 写完所有单元日志
                //unitLoggerModule.WriteAllLogs();
            });
            lifetime.ApplicationStopped.Register(()=>{
                loggerModule.Log(LogLevel.Warning,"Startup.Configure","winds 服务主机已停止");
            });
        }
    }
}
