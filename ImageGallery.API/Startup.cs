using IdentityServer4.AccessTokenValidation;
using ImageGallery.API.Entities;
using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Web
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

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = Configuration["IdentityAuthority"];
                options.ApiName = "imagegalleryapi";
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("MustOwnImage",
                policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.AddRequirements(new ImageGallery.API.Authorization.Requirements.MustOwnImage());
                });
            });

            services.AddScoped<IAuthorizationHandler, ImageGallery.API.Authorization.Handlers.MustOwnImage>();

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:imageGalleryDBConnectionString"];
            services.AddDbContext<GalleryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<IGalleryRepository, GalleryRepository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
    ILoggerFactory loggerFactory, GalleryContext galleryContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }


            app.UseHttpsRedirection();


            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePages(async context =>
                {
                    context.HttpContext.Response.ContentType = "text/plain";

                    await context.HttpContext.Response.WriteAsync(
                        "Status code page, status code: " +
                        context.HttpContext.Response.StatusCode);
                });
            }

            app.UseAuthentication();
            app.UseStaticFiles();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                // Map from Image (entity) to Image, and back
                cfg.CreateMap<Image, ImageGallery.Model.Image>().ReverseMap();

                // Map from ImageForCreation to Image
                // Ignore properties that shouldn't be mapped
                cfg.CreateMap<ImageGallery.Model.ImageForCreation, Image>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());

                // Map from ImageForUpdate to Image
                // ignore properties that shouldn't be mapped
                cfg.CreateMap<ImageGallery.Model.ImageForUpdate, Image>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());
            });

            AutoMapper.Mapper.AssertConfigurationIsValid();

            // ensure DB migrations are applied
            galleryContext.Database.Migrate();

            // seed the DB with data
            galleryContext.EnsureSeedDataForContext();

            app.UseMvc();
        }


    }
}
