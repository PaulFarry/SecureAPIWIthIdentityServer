﻿using IdentityServer4;
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
                    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                    Username = "Frank",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim(IdentityServerConstants.StandardScopes.Address, "Main Road 1"),
                        new Claim("role", "FreeUser"),
                    }
                },
                new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "Claire",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim(IdentityServerConstants.StandardScopes.Address, "Big Street 2"),
                        new Claim("role", "PaidUser"),
                    }
                }
            };
        }
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
                {
                    new ApiResource(ImageGalleryApi, "Image Gallery API", new List<string>{ "role" })
                };
        }

        private static readonly string ImageGalleryApi = "imagegalleryapi";

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource(
                    "roles",
                    "Your roles(s)",
                    new List<string>
                    {
                        "role"
                    }),

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
                        "roles",
                     ImageGalleryApi
                   }
                }
        };
        }
    }
}
