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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.Options;
using backend.Models;
using backend.Context;
using backend.Service;
using backend.Settings;
using Microsoft.OpenApi;
using System.IO;

namespace backend
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "backend", Version = "v1" });
            });
            services.AddOptions<RegistrationOptions>()
                .Bind(Configuration.GetSection("RegistrationOptions"));
            if ("postgresql" == Configuration["ConnectionStrings:SqlType"])
            {
                services.AddDbContext<GamerzillaContext>(
                    options => options.UseNpgsql(Configuration["ConnectionStrings:TrophyConnection"]));
            }
            else
            {
                services.AddDbContext<GamerzillaContext>(
                    options => options.UseSqlite(Configuration["ConnectionStrings:TrophyConnection"]));
            }
            services.AddScoped<SessionContext>();
            services.AddScoped<GamerzillaService>();
            services.AddScoped<UserService>();
            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.AddTransient<EmailService>();
            string frontend = Configuration["Frontend"] ?? "";
            bool isHttps = frontend.StartsWith("https", StringComparison.OrdinalIgnoreCase);

            services.AddCors(options =>
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();

                    if (!string.IsNullOrWhiteSpace(frontend))
                    {
                        builder.WithOrigins(frontend.Split(','));
                    }
                    else
                    {

                        builder.SetIsOriginAllowed(_ => true);
                    }
                }));


            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
            }).AddCookie("Cookies", options =>
            {
                options.Cookie.Name = "auth_cookie";
                options.Cookie.Path = "/";



                options.Cookie.SecurePolicy = isHttps
                    ? CookieSecurePolicy.Always
                    : CookieSecurePolicy.SameAsRequest;

                options.Cookie.SameSite = isHttps
                    ? SameSiteMode.None
                    : SameSiteMode.Lax;

                Console.WriteLine($"🍪 Cookie Policy: HTTPS Detected={isHttps}, Secure={options.Cookie.SecurePolicy}, SameSite={options.Cookie.SameSite}");

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


        private string _cachedHtml = null;
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var basePath = Configuration["BasePath"];
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                var cleanPath = basePath.StartsWith("/") ? basePath : "/" + basePath;
                cleanPath = cleanPath.TrimEnd('/');

                Console.WriteLine($"🚀 Application running under base path: {cleanPath}");

                app.UsePathBase(cleanPath);
            }


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

            }


            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallback(async (context) =>
                {

                    if (_cachedHtml != null && !_env.IsDevelopment())
                    {
                        context.Response.ContentType = "text/html";
                        await context.Response.WriteAsync(_cachedHtml);
                        return;
                    }



                    var basePath = Configuration["BasePath"]?.TrimEnd('/') ?? "";
                    if (!basePath.StartsWith("/") && !string.IsNullOrEmpty(basePath))
                    {
                        basePath = "/" + basePath;
                    }


                    var baseHref = string.IsNullOrEmpty(basePath) ? "/" : basePath + "/";

                    string frontendSetting = Configuration["Frontend"] ?? "";
                    string frontendUrl = !string.IsNullOrWhiteSpace(frontendSetting)
                        ? frontendSetting.Split(',')[0].TrimEnd('/')
                        : $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";


                    var filePath = Path.Combine(_env.WebRootPath, "index.html");

                    if (!File.Exists(filePath))
                    {
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync("index.html not found in wwwroot");
                        return;
                    }

                    var html = await File.ReadAllTextAsync(filePath);


                    var injection = $@"
    <base href=""{baseHref}"" />
    <script>
      window.APP_CONFIG = {{
        basePath: '{basePath}',
        apiUrl: '{frontendUrl}/api'
      }};
    </script>";


                    var finalHtml = html.Replace("<head>", "<head>" + injection, StringComparison.OrdinalIgnoreCase);


                    if (!_env.IsDevelopment())
                    {
                        _cachedHtml = finalHtml;
                    }

                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(finalHtml);
                });
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {

                    var context = services.GetRequiredService<GamerzillaContext>();


                    context.Database.EnsureCreated();

                    Console.WriteLine("✅ Databases verified/created successfully.");
                }
                catch (Exception ex)
                {

                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "❌ An error occurred creating the DB.");
                }
            }

        }
    }
}
