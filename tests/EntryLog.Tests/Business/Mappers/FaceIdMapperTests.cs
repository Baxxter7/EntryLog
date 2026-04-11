using EntryLog.Business.Mappers;
using EntryLog.Entities.POCOEntities;
using FluentAssertions;

namespace EntryLog.Tests.Business.Mappers;

public class FaceIdMapperTests
{
    [Fact]
    public void MapToEmployeeFaceIdDto_MapsFieldsCorrectly()
    {
        var faceId = new FaceID
        {
            ImageUrl = "https://img.test/face.png",
            RegisterDate = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            Active = true
        };

        var dto = FaceIdMapper.MapToEmployeeFaceIdDto(faceId, "base64data");

        dto.Base64Image.Should().Be("base64data");
        dto.Active.Should().BeTrue();
        dto.RegisterDate.Should().Be("15/01/2026 06:00 AM");
    }

    [Fact]
    public void Empty_ReturnsEmptyDto()
    {
        var dto = FaceIdMapper.Empty();

        dto.Base64Image.Should().BeEmpty();
        dto.RegisterDate.Should().BeEmpty();
        dto.Active.Should().BeFalse();
    }
}
