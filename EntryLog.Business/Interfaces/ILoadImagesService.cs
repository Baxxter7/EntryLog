using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces;

internal interface ILoadImagesService
{
    //Task<ImageBBResponseDto> UploadAsync(Stream image, string fileName, string extension);
    Task<string?> UploadAsync(Stream image, string type, string fileName);
}
