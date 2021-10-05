using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ProjectManagement.AuthenticationHandlers;
using ProjectManagement.BusinessLogic.Services.Implementation;
using ProjectManagement.BusinessLogic.Services.Interfaces;
using ProjectManagement.DataAccess.Context;
using ProjectManagement.DataAccess.Repositories.Implementation;
using ProjectManagement.DataAccess.Repositories.Interfaces;
using ProjectManagement.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


//using FluentValidation.AspNetCore;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;

//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.OpenApi.Models;



namespace ProjectManagement
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
            services.AddDbContext<ProjectManagementContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("ProjectManagement")));

            services.AddScoped<ICheckListRepository, CheckListRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBoardRepository, BoardRepository>();

            services.AddScoped<ICheckListService, CheckListService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IBoardService, BoardService>();

            services.AddAuthentication("Basic")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", null);

            services.AddControllers()
                .AddFluentValidation(fvc =>
                {
                    fvc.RegisterValidatorsFromAssemblyContaining<Startup>();
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectManagement", Version = "v1" });
                c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authorization header using the Bearer scheme."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "basic"
                                }
                            },
                            new string[] {}
                    }
                });
            });

            services.AddAutoMapper(typeof(CheckListProfile));
            //services.AddAutoMapper(typeof(CheckListProfile));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectManagement v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    //public class Startup
    //{
    //    public Startup(IConfiguration configuration)
    //    {
    //        Configuration = configuration;
    //    }

    //    public IConfiguration Configuration { get; }

    //    // This method gets called by the runtime. Use this method to add services to the container.
    //    public void ConfigureServices(IServiceCollection services)
    //    {

    //        services.AddControllers();
    //        services.AddSwaggerGen(c =>
    //        {
    //            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProjectManagement", Version = "v1" });
    //        });
    //    }

    //    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    //    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    //    {
    //        if (env.IsDevelopment())
    //        {
    //            app.UseDeveloperExceptionPage();
    //            app.UseSwagger();
    //            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProjectManagement v1"));
    //        }

    //        app.UseHttpsRedirection();

    //        app.UseRouting();

    //        app.UseAuthorization();

    //        app.UseEndpoints(endpoints =>
    //        {
    //            endpoints.MapControllers();
    //        });
    //    }
    //}
}
