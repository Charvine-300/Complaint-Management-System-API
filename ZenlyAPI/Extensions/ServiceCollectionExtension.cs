using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ZenlyAPI.Context;
using ZenlyAPI.Domain.Config;
using ZenlyAPI.Services.CourseMgmt;
using ZenlyAPI.Services.DepartmentMgmt;
using ZenlyAPI.Services.FacultyMgmt;

namespace ZenlyAPI.Extensions
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Register services to the DI
        /// <param name="services"></param>
        /// </summary>
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.AddScoped<ICourseMgmtService, CourseMgmtService>();
            services.AddScoped<IFacultyMgmtService, FacultyMgmtService>();
            services.AddScoped<IDepartmentMgmtService, DepartmentMgmtService>();

            services.AddScoped<IMemoryCache, MemoryCache>();
        }

        /// <summary>
        /// Configure lazy loading of navigation property, Register SqlServer
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString"></param>
        public static void RegisterDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ZenlyDbContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseSqlServer(connectionString, s =>
                {
                    s.EnableRetryOnFailure(3);
                });
            });
        }

        /// <summary>
        /// Register authentications with jwt.
        /// </summary>
        /// <param name="services"></param>
        public static void RegisterAuthentication(this IServiceCollection services)
        {
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(24));

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {

                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = "auth_cookie";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;


                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization();
        }

        /// <summary>
        /// Binds the configuration file to the CreditCardConfig class which has the same section names and structure with the CreditCardConfig section in the configuration file.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ZenlyConfig BindConfiguration(this IServiceCollection services, IConfiguration configuration)
        {


            ZenlyConfig appConfig = new();
            configuration.GetSection(nameof(ZenlyConfig)).Bind(appConfig);
            services.AddSingleton(appConfig);
            return appConfig;
        }
    }
}
