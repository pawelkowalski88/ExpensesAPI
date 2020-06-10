using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ExpensesAPI.IdentityProvider.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityServer4.Services;
using ExpensesAPI.IdentityProvider.Repositories;
using System.Reflection;
using IdentityServer4.Validation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System;

namespace ExpensesAPI.IdentityProvider
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection"),
                    b => { 
                        b.MigrationsHistoryTable("_EFMigrationsHistoryTableIdentity");
                    }));

            services.AddDefaultIdentity<User>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddRazorPages();
            services.AddControllers();

            services.AddTransient<IProfileService, IdentityProfileService>();

            services.AddIdentityServer()
                //.AddInMemoryIdentityResources(Config.Ids)
                //.AddInMemoryApiResources(Config.Apis)
                //.AddInMemoryClients(Config.Clients)
                .AddConfigurationStore(o =>
                {
                    // o.ConfigureDbContext = builder =>
                    //     builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    //         b => {
                    //             b.MigrationsHistoryTable("_EFMigrationsHistoryConfigurationStore");
                    //             b.MigrationsAssembly("ExpensesAPI.IdentityProvider");
                    //         });

                    o.ConfigureDbContext = builder =>
                        builder.UseSqlite("Data Source=ConfigurationDatabase.db",                   
                            b => {
                                b.MigrationsHistoryTable("_EFMigrationsHistoryConfigurationStore");
                                b.MigrationsAssembly("ExpensesAPI.IdentityProvider");
                            });
                })
                .AddOperationalStore(o =>
                {
                    // o.ConfigureDbContext = builder =>
                    //     builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    //         b =>
                    //         {
                    //             b.MigrationsHistoryTable("_EFMigrationsHistoryCOperationalStore");
                    //             b.MigrationsAssembly("ExpensesAPI.IdentityProvider");
                    //         });

                    o.ConfigureDbContext = builder =>
                        builder.UseSqlite("Data Source=OperationalDatabase.db",                   
                            b => {
                                b.MigrationsHistoryTable("_EFMigrationsHistoryConfigurationStore");
                                b.MigrationsAssembly("ExpensesAPI.IdentityProvider");
                            });

                    o.EnableTokenCleanup = true;
                    o.TokenCleanupInterval = 30;
                })
                .AddDeveloperSigningCredential()
                .AddAspNetIdentity<User>()
                .AddProfileService<IdentityProfileService>();

            services.AddTransient<IUserRepository, UserRepository>();
            services.AddScoped<IExtensionGrantValidator, DelegationGrantValidator>();

            services.AddAutoMapper(Assembly.Load("ExpensesAPI.IdentityProvider"));

            services.AddCors(opts =>
            {
                opts.AddPolicy(name: "CORSPolicy",
                     builder =>
                     {
                         builder.WithOrigins(Configuration["CORS"])
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                     });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitializeDatabase(app);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("CORSPolicy");

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();

            //app.UseMvcWithDefaultRoute();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            //app.ApplicationServices.GetService(DbContext<)
        }
    }
}
