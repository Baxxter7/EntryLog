using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces;

internal interface ILoadImagesService
{
    Task<ImageBBResponseDto> UploadAsync(Stream image, string fileName, string extension);
}
