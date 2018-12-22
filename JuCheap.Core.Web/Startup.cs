using Hangfire;
using Hangfire.MySql.Core;
using JuCheap.Core.Data;
using JuCheap.Core.Infrastructure.Utilities;
using JuCheap.Core.Interfaces;
using JuCheap.Core.Services;
using JuCheap.Core.Web.Filters;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace JuCheap.Core.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var repository = LogManager.CreateRepository(Constants.Log4net.RepositoryName);
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.ExpireTimeSpan = TimeSpan.FromMinutes(480);//cookieĬ����Чʱ��Ϊ8��Сʱ
                    o.LoginPath = new PathString("/Home/Login");
                    o.LogoutPath = new PathString("/Home/Logout");
                    o.Cookie = new CookieBuilder
                    {
                        HttpOnly = true,
                        Name = ".JuCheap.Core.Identity",
                        Path = "/"
                    };
                    //o.DataProtectionProvider = null;//�����Ҫ�����ؾ��⣬����Ҫ�ṩһ��Key
                });
            //ʹ��Sql Server���ݿ�
            //services.AddDbContext<JuCheapContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Connection_SqlServer")));

            ////ʹ��Sqlite���ݿ�
            //services.AddDbContext<JuCheapContext>(options => options.UseSqlite(Configuration.GetConnectionString("Connection_Sqlite")));

            //ʹ��MySql���ݿ�
            services.AddDbContext<JuCheapContext>(options => options.UseMySql(Configuration.GetConnectionString("Connection_MySql")));

            //Ȩ����֤filter
            services.AddMvc(cfg =>
            {
                cfg.Filters.Add(new RightFilter());
            });            
            //.AddJsonOptions(option => option.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver());//���ô�Сд���⣬Ĭ��������ĸСд

            // service����ע��
            services.UseJuCheapService();

            //hangfire�Զ������������ݿ�����
            //ʹ��sql server���ݿ���hangfire�ĳ־û�
            //services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("Connection_Job_SqlServer")));

            //ʹ��mysql���ݿ���hangfire�ĳ־û�
            //ʹ��mysql��ʱ�����ں�ͬѧ��֪����ô�������ݿ�����ݱ�ı����ʽ���ᵼ��hangfire��ʼ��ʧ�ܣ�
            //���Բ��Զ�����hangfire�����ݱ���Ҫ�ֶ������Ŀ¼��Hangfire/hangfire.job.mysql.sql�ļ���������
            var mySqlOption = new MySqlStorageOptions
            {
                PrepareSchemaIfNecessary = false
            };
            services.AddHangfire(x => x.UseStorage(new MySqlStorage(Configuration.GetConnectionString("Connection_Job_MySql"),mySqlOption)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            //ȫ�������֤
            app.UseAuthentication();
            //���ʼ�¼middleware
            app.UseMiddleware<VisitMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //��ʼ�����ݿ��Լ���ʼ����
            Task.Run(async () =>
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var menues = MenuHelper.GetMenues();
                    var dbService = scope.ServiceProvider.GetService<IDatabaseInitService>();
                    await dbService.InitAsync(menues);
                }
            });

            //hangfire�Զ���������
            var jobOptions = new BackgroundJobServerOptions
            {
                ServerName = Environment.MachineName,
                WorkerCount = 1
            };
            app.UseHangfireServer(jobOptions);
            var option = new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            };
            app.UseHangfireDashboard("/task", option);
            //���һ��ÿ���Զ����賿��ʱ��ִ�е�ͳ������
            RecurringJob.AddOrUpdate<ISiteViewService>(x => x.AddOrUpdate(), Cron.Daily());
            RecurringJob.AddOrUpdate(() => Console.WriteLine($"Job��{DateTime.Now}ִ�����."), Cron.Minutely());
        }
    }    

    /// <summary>
    /// IIdentity��չ
    /// </summary>
    public static class IdentityExtention
    {
        /// <summary>
        /// ��ȡ��¼���û�ID
        /// </summary>
        /// <param name="identity">IIdentity</param>
        /// <returns></returns>
        public static string GetLoginUserId(this IIdentity identity)
        {
            var claim = (identity as ClaimsIdentity)?.FindFirst("LoginUserId");
            if (claim != null)
            {
                return claim.Value;
            }
            return string.Empty;
        }
    }
}
