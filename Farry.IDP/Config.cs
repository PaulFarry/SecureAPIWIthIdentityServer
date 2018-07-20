using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace Farry.IDP
{
    public static class Config
    {
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "E78E0654-2F41-42AB-B71C-D37C8341C597",
                    Username = "Frank",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim(IdentityServerConstants.StandardScopes.Address, "Main Road 1")
                    }
                },
                new TestUser
                {
                    SubjectId = "EE81E65A-91C0-46EA-A66F-2542DF19A749",
                    Username = "Claire",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim(IdentityServerConstants.StandardScopes.Address, "Big Street 2")
                    }
                }
            };
        }
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
                {
                    new ApiResource("api1", "My API")
                };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address()
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client> {
             new Client
                {
                    ClientId = "imagegalleryclient",
                    ClientName  = "Image Gallery",

                    // no interactive user, use the clientid/secret for authentication
                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RedirectUris = new List<string> {
                    "https://localhost:44360/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>{
                    "https://localhost:44360/signout-callback-oidc"
                    },
                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                    }
                }
        };
        }
    }
}
