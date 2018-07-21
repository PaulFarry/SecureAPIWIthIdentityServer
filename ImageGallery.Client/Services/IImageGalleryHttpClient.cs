using System.Net.Http;
using System.Threading.Tasks;

namespace ImageGallery.Client.Services
{
    public interface IImageGalleryHttpClient
    {
        HttpClient GetClient();
    }
}
