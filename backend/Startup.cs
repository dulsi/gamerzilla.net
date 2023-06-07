using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.Options;
using backend.Models;
using backend.Context;
using backend.Service;
using backend.Settings;

namespace backend
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "backend", Version = "v1" });
            });
            services.AddOptions<RegistrationOptions>()
                .Bind(Configuration.GetSection("RegistrationOptions"));
            if ("sqlserver" == Configuration["ConnectionStrings:SqlType"])
            {
                services.AddDbContext<GamerzillaContext>(
                    options => options.UseSqlServer(Configuration["ConnectionStrings:TrophyConnection"]));
                services.AddDbContext<UserContext>(
                    options => options.UseSqlServer(Configuration["ConnectionStrings:UserConnection"]));
            }
            else if ("postgresql" == Configuration["ConnectionStrings:SqlType"])
            {
                services.AddDbContext<GamerzillaContext>(
                    options => options.UseNpgsql(Configuration["ConnectionStrings:TrophyConnection"]));
                services.AddDbContext<UserContext>(
                    options => options.UseNpgsql(Configuration["ConnectionStrings:UserConnection"]));
            }
            else
            {
                services.AddDbContext<GamerzillaContext>(
                    options => options.UseSqlite(Configuration["ConnectionStrings:TrophyConnection"]));
                services.AddDbContext<UserContext>(
                    options => options.UseSqlite(Configuration["ConnectionStrings:UserConnection"]));
            }
            services.AddScoped<SessionContext>();
            services.AddScoped<UserService>();
            services.AddCors(options =>
                options.AddPolicy("CorsPolicy", builder =>
                    builder.AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins(Configuration["Frontend"].Split(','))
                        .AllowCredentials()));
            services.AddAuthentication(options => {
                options.DefaultScheme = "Cookies";
            }).AddCookie("Cookies", options => {
                options.Cookie.Name = "auth_cookie";
                options.Cookie.SameSite = SameSiteMode.None;
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = redirectContext =>
                    {
                        redirectContext.HttpContext.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseCors("CorsPolicy");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "backend v1"));
            }
            else
            {
//                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
