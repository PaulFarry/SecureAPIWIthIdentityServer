using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using IdentityModel.Client;
using System.Collections.Generic;
using System.Globalization;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ImageGalleryHttpClient(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _configuration = configuration;

            var apiAddress = configuration["ApiLocation"];

            _httpContextAccessor = httpContextAccessor;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiAddress)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        public async Task<HttpClient> GetClient()
        {
            var accessToken = string.Empty;

            var currentContext = _httpContextAccessor.HttpContext;
            var expires_at = await currentContext.GetTokenAsync("expires_at");

            if (
            string.IsNullOrWhiteSpace(expires_at) 
            || 
            DateTime.Parse(expires_at).AddSeconds(-60).ToUniversalTime() < DateTime.UtcNow)
            {
                accessToken = await RenewTokens();
            }
            else
            {
                accessToken = await currentContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                _httpClient.SetBearerToken(accessToken);
            }
            return _httpClient;
        }

        private async Task<string> RenewTokens()
        {
            var currentContext = _httpContextAccessor.HttpContext;

            var disco = new DiscoveryClient(_configuration["IdentityAuthority"]);

            var metaDataResponse = await disco.GetAsync();
            var tokenClient = new TokenClient(metaDataResponse.TokenEndpoint, "imagegalleryclient", "secret");
            var currentRefreshToken = await currentContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            var tokenResult = await tokenClient.RequestRefreshTokenAsync(currentRefreshToken);

            if (!tokenResult.IsError)
            {
                var updatedTokens = new List<AuthenticationToken>();
                updatedTokens.Add(new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.IdToken,
                    Value = tokenResult.IdentityToken
                });

                updatedTokens.Add(new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.AccessToken,
                    Value = tokenResult.AccessToken
                });
                updatedTokens.Add(new AuthenticationToken
                {
                    Name = OpenIdConnectParameterNames.RefreshToken,
                    Value = tokenResult.RefreshToken
                });

                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                updatedTokens.Add(new AuthenticationToken
                {
                    Name = "expires_at",
                    Value = expiresAt.ToString("o", CultureInfo.InvariantCulture)
                });

                //get the current authentication result with principal and properties
                var currentAuthenticateResult = await currentContext.AuthenticateAsync("Cookies");
                //store the updated tokens
                currentAuthenticateResult.Properties.StoreTokens(updatedTokens);

                //sign in with the new details
                await currentContext.SignInAsync("Cookies", currentAuthenticateResult.Principal, currentAuthenticateResult.Properties);

                //return the new access token
                return tokenResult.AccessToken;

            }
            else
            {
                throw new Exception("Problems whilst refreshing tokens", tokenResult.Exception);
            }
        }
    }
}

