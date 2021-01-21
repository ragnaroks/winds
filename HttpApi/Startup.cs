using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            services.AddSingleton<Modules.UserManageModule>().Configure<Options.UserManageModuleOptions>(options=>{
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
        public void Configure(IApplicationBuilder app,IWebHostEnvironment env,IHostApplicationLifetime lifetime,ILogger<Startup> logger,
            Modules.UserManageModule userManageModule,
            Filters.RequireUserAttribute requireUserAttribute,Filters.RequireManagerAttribute requireManageAttributer){
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                Program.Settings.SetDevelopMode(true);
            } else {
                app.UseExceptionHandler("/Default/Error");
            }
            app.UseCors();
            app.UseRouting();
            app.UseEndpoints(endpoints=>{
                endpoints.MapControllerRoute(name:"default",pattern:"{controller=Default}/{action=Index}/{id?}");
            });

            //生命周期
            lifetime.ApplicationStarted.Register(()=>{
                logger.LogWarning("winds 服务主机已启动");
                //预热
                _=userManageModule.GetHashCode();
                //_=unitManageModule.GetHashCode();
                //_=unitLoggerModule.GetHashCode();
                _=requireUserAttribute.GetHashCode();
                _=requireManageAttributer.GetHashCode();
            });
            lifetime.ApplicationStopping.Register(()=>{
                logger.LogWarning("winds 服务主机正在停止");
                // 停止所有单元
                //unitManageModule.StopAllUnits();
                // 写完所有单元日志
                //unitLoggerModule.WriteAllLogs();
            });
            lifetime.ApplicationStopped.Register(()=>{
                logger.LogWarning("winds 服务主机已停止");
            });
        }
    }
}
