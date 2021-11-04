using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
using Microsoft.AspNetCore.Http;

using System.Text.Json.Serialization;

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
            services.AddScoped<ICardRepository, CardRepository>();
            services.AddScoped<IListRepository, ListRepository>();
            services.AddControllersWithViews()
            .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddScoped<IBoardService, BoardService>(x =>
                        new BoardService(
                            x.GetRequiredService<IBoardRepository>(), x.GetRequiredService<IUserRepository>(),
                            x.GetRequiredService<IBoardMemberRepository>(), x.GetRequiredService<IUserManager>()));
            services.AddScoped<ICardService, CardService>(x =>
                        new CardService(
                            x.GetRequiredService<ICardRepository>(), x.GetRequiredService<IListRepository>(),
                            x.GetRequiredService<ICardMemberRepository>(), x.GetRequiredService<IUserManager>()));
            services.AddScoped<ICheckListService, CheckListService>(x =>
                        new CheckListService(x.GetRequiredService<ICheckListRepository>(),
                        x.GetRequiredService<ICardRepository>(), x.GetRequiredService<IUserManager>()));
            services.AddScoped<IBoardMemberRepository, BoardMemberRepository>(x =>
                        new BoardMemberRepository(x.GetRequiredService<ProjectManagementContext>()));
            services.AddScoped<ICardMemberRepository, CardMemberRepository>(x =>
                        new CardMemberRepository(x.GetRequiredService<ProjectManagementContext>()));
            services.AddScoped<IUserManager, ProjectManagement.BusinessLogic.Services.Implementation.UserMananger>(x =>
                        new UserMananger(
                            x.GetRequiredService<IHttpContextAccessor>(), x.GetRequiredService<IUserRepository>()));
            services.AddScoped<IUserService, UserService>();
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
                c.CustomSchemaIds(i => i.FullName);
                c.DocInclusionPredicate((docName, description) => true);
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
            services.AddAutoMapper(typeof(ProjectManagementProfile));
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
}
