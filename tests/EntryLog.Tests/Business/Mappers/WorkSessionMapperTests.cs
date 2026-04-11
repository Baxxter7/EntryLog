using EntryLog.Business.Mappers;
using EntryLog.Entities.POCOEntities;
using EntryLog.Tests.Helpers;
using FluentAssertions;

namespace EntryLog.Tests.Business.Mappers;

public class WorkSessionMapperTests
{
    [Fact]
    public void MapToGetWorkSessionDto_MapsCheckInCorrectly()
    {
        var session = new WorkSessionBuilder().WithEmployeeId(1001).Build();

        var dto = WorkSessionMapper.MapToGetWorkSessionDto(session);

        dto.EmployeeId.Should().Be(1001);
        dto.Status.Should().Be("InProgress");
        dto.CheckIn.Should().NotBeNull();
        dto.CheckIn.Method.Should().Be(session.CheckIn.Method);
        dto.CheckIn.Location.Latitude.Should().Be(session.CheckIn.Location.Latitude);
        dto.CheckOut.Should().BeNull();
        dto.TotalWorked.Should().BeNull();
    }

    [Fact]
    public void MapToGetWorkSessionDto_WithCheckOut_MapsBothChecks()
    {
        var checkOut = new Check
        {
            Method = "Agent",
            DeviceName = "Windows",
            Date = DateTime.UtcNow,
            Location = new Location { Latitude = "14.0", Longitude = "-90.0", IpAddress = "10.0.0.1" },
            PhotoUrl = "https://img.test/out.jpg"
        };
        var session = new WorkSessionBuilder().WithCheckOut(checkOut).Build();

        var dto = WorkSessionMapper.MapToGetWorkSessionDto(session);

        dto.CheckOut.Should().NotBeNull();
        dto.Status.Should().Be("Completed");
        dto.TotalWorked.Should().NotBeNull();
    }
}
