using EntryLog.Business.Constants;
using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EntryLog.Business.ImageBB;

internal class ImageBBService : ILoadImagesService
{
    private readonly ImageBBOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public ImageBBService(IOptions<ImageBBOptions> options, IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string?> UploadAsync(
    Stream image,
    string contentType,
    string fileName
    )
    {
        using HttpClient client = _httpClientFactory.CreateClient(ApiNames.ImageBB);

        using var content = CreateMultipartContent(image, contentType, fileName);

        HttpResponseMessage response = await client.PostAsync(
            BuildUploadUrl(),
            content
        );

        if (!response.IsSuccessStatusCode)
            return null;

        string responseBody = await response.Content.ReadAsStringAsync();

        ImageBBResponseDto imageBBResponse = JsonSerializer.Deserialize<ImageBBResponseDto>(responseBody);

        return imageBBResponse.Data.Url;
    }

    private MultipartFormDataContent CreateMultipartContent(
        Stream image,
        string contentType,
        string fileName)
    {
        var streamContent = new StreamContent(image);
        streamContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        var form = new MultipartFormDataContent();
        form.Add(streamContent, "image", fileName);

        return form;
    }

    private string BuildUploadUrl()
    {
        return $"/1/upload?expiration={_options.ExpirationSeconds}&key={_options.ApiToken}";
    }

}
