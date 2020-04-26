using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ringer.HubServer.Data;
using Microsoft.EntityFrameworkCore;
using System;
using Ringer.HubServer.Services;
using Ringer.HubServer.Hubs;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace Ringer.HubServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<RingerDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("RingerDbContext")));

            // security key
            string securityKey = Configuration["SecurityKey"];

            // symmmetric security key
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // What to validate
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // validate expire time
                    ValidateIssuerSigningKey = true,

                    // setup validate data
                    ValidIssuer = "Ringer",
                    ValidAudience = "ringer.co.kr",
                    IssuerSigningKey = symmetricSecurityKey
                };

                // TODO: Configute the authority to the expected value for your authentication provider
                // This ensures the token is appropriately validated
                // options.Authority = "http://localhost:5000/auth/token";
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub..
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/Hubs/Chat")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSpaStaticFiles(options =>
            {
                options.RootPath = "wwwroot";
            });

            MailConfigSection mailConfigSection = Configuration.GetSection("mailConfigSection").Get<MailConfigSection>();
            services.AddSingleton(mailConfigSection);
            //services.AddScoped<IEmailSender, MailgunEmailSender>();
            services.AddHttpClient<IEmailSender, MailgunEmailSender>(cfg =>
            {
                cfg.BaseAddress = new Uri("https://api.mailgun.net");
                cfg.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{mailConfigSection.MailgunKey}")));
            });

            services.AddControllers();

            services.AddSignalR();

            // configure DI for application services
            services.AddScoped<IUserService, UserService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Ringer API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                                Enter 'Bearer' [space] and then your token in the text input below.
                                \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseCors("allow");

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ringer API V1");
            });

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/Hubs/Chat");
                endpoints.MapControllers();
            });

            app.UseSpaStaticFiles();
            app.UseSpa(builder =>
            {
                if (env.IsDevelopment())
                {
                    builder.UseProxyToSpaDevelopmentServer("http://localhost:8080");
                }
            });
        }
    }
}
