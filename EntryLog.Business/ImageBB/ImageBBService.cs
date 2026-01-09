using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using Microsoft.Extensions.Options;

namespace EntryLog.Business.ImageBB;

internal class ImageBBService : ILoadImagesService
{
    private readonly IOptions<ImageBBOptions> _options;

    public ImageBBService(IOptions<ImageBBOptions> options)
    {
        _options = options;
    }

    public Task<ImageBBResponseDto> UploadAsync(Stream image, string fileName, string extension)
    {
        throw new NotImplementedException();
    }
}
