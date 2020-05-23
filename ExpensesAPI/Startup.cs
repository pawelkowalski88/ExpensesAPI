using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ExpensesAPI.Domain.Persistence;
using System.Reflection;
using Newtonsoft.Json;
using Expenses.FileImporter;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using ExpensesAPI.Domain.ExternalAPIUtils;
using ExpensesAPI.Domain.Models;

namespace ExpensesAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var database = Configuration.GetSection("Database")["Vendor"];

            switch (database)
            {
                case "mysql":
                    services.AddDbContext<MainDbContext>(options => options.UseMySql(Configuration.GetConnectionString("MySQL")));
                    break;
                case "sqlite":
                    services.AddDbContext<MainDbContext>(options => options.UseSqlite("Data Source=expenses.db"));
                    break;
                case "mssqlserver":
                    services.AddDbContext<MainDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default"),
                        b =>
                        {
                            b.MigrationsAssembly("ExpensesAPI.Domain");
                            b.MigrationsHistoryTable("_EFMigrationsHistoryAPI");
                        }));
                    break;

                default:
                    throw new System.Exception("No database configured.");
            }

            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUnitOfWork, EFUnitOfWork>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IScopeRepository, ScopeRepository>();
            services.AddScoped<IUserRepository<User>, UserRepository>();
            services.AddScoped<IUserRepository<IdentityServerUser>, UserRepositoryExternalApi>();
            services.AddScoped<IFileImporter, CSVImporter>();

            services.AddScoped<ITokenRepository, LastTokenRepository>();

            services.AddAutoMapper(Assembly.Load("ExpensesAPI.Domain"), Assembly.Load("ExpensesAPI.SimpleJWTAuth"));

            //services.AddSimpleJWTAuth<User, MainDbContext>(Configuration);

            //services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        options.Authority = "https://localhost:5000";
            //        options.Audience = "ExpensesAPI";
            //    });


            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(o =>
                {
                    o.Authority = Configuration["IdentityProviderURL"];
                    o.ApiName = "ExpensesAPI";
                    o.RequireHttpsMetadata = false;
                });

            services.AddControllers(o =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                o.Filters.Add(new AuthorizeFilter(policy));
            })
                .AddNewtonsoftJson(o => { o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; });



            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                b =>
                {
                    b.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
