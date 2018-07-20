﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public ImageGalleryHttpClient(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            var data = configuration["ApiLocation"];

            _httpContextAccessor = httpContextAccessor;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(data)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        public HttpClient GetClient()
        {
            return _httpClient;
        }
    }
}
