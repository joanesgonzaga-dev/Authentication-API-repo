using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Softaway.Identidade.API.Data;
using Softaway.Identidade.API.Extensions;
using System;
using System.Text;

namespace Softaway.Identidade.API
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
            #region EF Core Connection Configuration
            services.AddDbContext<ApplicationDbContext>(options=> 
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            #endregion

            #region Identity User Authentication Configuration
            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders(); //Nao tem a ver com o JWT
            #endregion

            #region JWT Authentication Scheme Configuration
            var AppSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(AppSettingsSection);

            var appSettings = AppSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                bearerOptions.RequireHttpsMetadata = true;
                bearerOptions.SaveToken = true;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    //ValidAudiences = ? (permite um IEnumerable de strings de audiencias)
                    ValidAudience = appSettings.ValidoEm,
                    ValidIssuer = appSettings.Emissor
                };
            });
            #endregion
            
            services.AddControllers();

            //Adds the Documentation generator middleware
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("V1", new OpenApiInfo 
                {
                    Title = "Mobster91 Enterprise Identity API",
                    Description = "Authentication Control Layer",
                    Contact = new OpenApiContact { Name = "Joanes Gonzaga", Email = "joanesgonzaga-dev@gmail.com"},
                    License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://www.opensource.org/licenses/mit")}
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/V1/swagger.json", "V1");
            });


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
