using AutoMapper;
using DuploAuth.Auth;
using DuploAuth.Helpers;
using DuploAuth.Models;
using DuploAuth.Models.Entities;
using DuploAuth.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DuploAuth
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
            services.AddIdentity<AppUser, IdentityRole>(o =>
            {
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            var path = "./file.db";
            if (isLinux)
            {
                path = "/app/file.db";
            }

            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlite("Filename=" + path));
            services.AddCors();
            services.AddSingleton<IJwtFactory, JwtFactory>();

            var key = Environment.GetEnvironmentVariable("APP_SECRET");
            if (Encoding.UTF8.GetByteCount(key) <= 120)
            {
                key = "xk1Z$S54Z%^wSM$uIYRgqPZ7LuqN7Cv9xrK5KHi&Bl3j^!88Ss7&H&";
            }
            var issuer = Environment.GetEnvironmentVariable("ISSUER");
            if (string.IsNullOrEmpty(issuer))
            {
                issuer = "duplo_cloud";
            }

            var _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = issuer;
                options.Audience = issuer;
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });

            services
                .AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
                {
                    o.SaveToken = true;
                    o.ClaimsIssuer = issuer;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = _signingKey,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = issuer,
                        ValidateIssuer = true,

                        ValidAudience = issuer,
                        ValidateAudience = true,

                        ClockSkew = TimeSpan.Zero,
                        RequireExpirationTime = false,
                        ValidateLifetime = true
                    };
                });

            services.AddAuthorization();
            services.AddAutoMapper();
            services.AddMvc(o =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                o.Filters.Add(new AuthorizeFilter(policy));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            app.UseCors(
                options => options
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
            );
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
            }
            DefaultInitializer.SeedRoles(roleManager);
            DefaultInitializer.SeedUsers(userManager);

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}