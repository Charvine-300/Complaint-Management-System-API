using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ZenlyAPI.Context;
using ZenlyAPI.Domain.Config;
using ZenlyAPI.Services.AuthMgmt;
using ZenlyAPI.Services.ComplaintsMgmt;
using ZenlyAPI.Services.ComplaintsTrailMgmt;
using ZenlyAPI.Services.CourseMgmt;
using ZenlyAPI.Services.DepartmentMgmt;
using ZenlyAPI.Services.FacultyMgmt;
using ZenlyAPI.Services.Shared.UserContextService;

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
            services.AddScoped<IComplaintsService, ComplaintsService>();
            services.AddScoped<IComplaintsTrailService,  ComplaintsTrailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserContextService,  UserContextService>();


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
        public static void RegisterAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(24));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtKey = configuration["ZenlyConfig:JwtConfig:JwtKey"];
                var issuer = configuration["ZenlyConfig:JwtConfig:JwtIssuer"];
                var audience = configuration["ZenlyConfig:JwtConfig:JwtAudience"];

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey!)
                    )
                };
            });

            services.AddAuthorization();
        }

        /// <summary>
        /// Binds the configuration file to the ZenlyConfig class which has the same section names and structure with the ZenlyConfig section in the configuration file.
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
