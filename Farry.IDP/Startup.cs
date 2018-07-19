using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Farry.IDP
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc();

            services
                .AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddTestUsers(Config.GetUsers())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients());

            services.AddAuthentication().AddOpenIdConnect("demoidsrv", "IdentityServer", options =>
                  {
                      options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                      options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                      options.Authority = "https://demo.identityserver.io/";
                      options.ClientId = "implicit";
                      options.ResponseType = "id_token";
                      options.SaveTokens = true;
                      options.CallbackPath = new PathString("/signin-idsrv");
                      options.SignedOutCallbackPath = new PathString("/signout-callback-idsrv");
                      options.RemoteSignOutPath = new PathString("/signout-idsrv");

                      options.TokenValidationParameters = new TokenValidationParameters
                      {
                          NameClaimType = "name",
                          RoleClaimType = "role"
                      };
                  });
            ;

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();

        }
    }
}
