using AutoMapper;
using ExpensesAPI.Domain.Models;
using ExpensesAPI.Domain;
using ExpensesAPI.SimpleJWTAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ExpensesAPI.Domain.Persistence;
using System.Reflection;

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
                    services.AddDbContext<MainDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));
                    break;

                default:
                    throw new System.Exception("No database configured.");
            }

            services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUnitOfWork, EFUnitOfWork>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IScopeRepository, ScopeRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            services.AddAutoMapper(Assembly.Load("ExpensesAPI"), Assembly.Load("ExpensesAPI.SimpleJWTAuth"));

            services.AddSimpleJWTAuth<User, MainDbContext>(Configuration);


            services.AddControllers();
                //.AddApplicationPart(Assembly.Load("ExpensesAPI.SimpleJWTAuth")).AddControllersAsServices();
            
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
