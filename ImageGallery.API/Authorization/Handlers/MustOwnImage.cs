using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.API.Authorization.Handlers
{
    public class MustOwnImage : AuthorizationHandler<Requirements.MustOwnImage>
    {
        private readonly IGalleryRepository _galleryRepository;

        public MustOwnImage(IGalleryRepository galleryRepository)
        {
            _galleryRepository = galleryRepository;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, Requirements.MustOwnImage requirement)
        {
            var filterContext = context.Resource as AuthorizationFilterContext;
            if (filterContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var id = filterContext.RouteData.Values["id"].ToString();


            if (!Guid.TryParse(id, out Guid imageId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var ownerId = context.User.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            if (!_galleryRepository.IsImageOwner(imageId, ownerId))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
