using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ringer.Backend.Hubs;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ringer.HubServer.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Ringer.HubServer
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
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                services.AddDbContext<RingerDbContext>(options =>
                        options.UseSqlServer(Configuration.GetConnectionString("RingerDbContext")));

                services.BuildServiceProvider().GetService<RingerDbContext>().Database.Migrate();
            }
            else
                services.AddDbContext<RingerDbContext>(options =>
                        options.UseSqlite(Configuration.GetConnectionString("RingerDbContext")));

            // security key
            string securityKey = "this_is_super_long_security_key_for_ringer_service";

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
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,

                    // setup validate data
                    ValidIssuer = "Ringer",
                    ValidAudience = "ringer.co.kr",
                    IssuerSigningKey = symmetricSecurityKey
                };

                // TODO: Configute the authority to the expected value for your authentication provider
                // This ensures the token is appropriately validated
                //options.Authority = "http://localhost:5000/auth/token";

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

            services.AddControllers();

            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/Hubs/Chat");
                endpoints.MapControllers();
            });
        }
    }
}
