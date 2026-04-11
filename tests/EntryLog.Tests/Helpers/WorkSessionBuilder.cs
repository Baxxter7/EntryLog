using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Tests.Helpers;

internal sealed class WorkSessionBuilder
{
    private Guid _id = Guid.NewGuid();
    private int _employeeId = 1001;
    private Check _checkIn = new()
    {
        Method = "Mozilla/5.0",
        DeviceName = "Windows",
        Date = DateTime.UtcNow.AddHours(-8),
        Location = new Location
        {
            Latitude = "14.6349",
            Longitude = "-90.5069",
            IpAddress = "127.0.0.1"
        },
        PhotoUrl = "https://example.com/checkin.jpg",
        Descriptor = Enumerable.Range(0, 128).Select(i => i * 0.01f).ToList()
    };
    private Check? _checkOut;
    private SessionStatus _status = SessionStatus.InProgress;

    public WorkSessionBuilder WithId(Guid id) { _id = id; return this; }
    public WorkSessionBuilder WithEmployeeId(int employeeId) { _employeeId = employeeId; return this; }
    public WorkSessionBuilder WithCheckIn(Check checkIn) { _checkIn = checkIn; return this; }
    public WorkSessionBuilder WithCheckOut(Check checkOut) { _checkOut = checkOut; _status = SessionStatus.Completed; return this; }
    public WorkSessionBuilder WithStatus(SessionStatus status) { _status = status; return this; }

    public WorkSession Build() => new()
    {
        Id = _id,
        EmployeeId = _employeeId,
        CheckIn = CloneCheck(_checkIn),
        CheckOut = _checkOut is null ? null : CloneCheck(_checkOut),
        Status = _status
    };

    private static Check CloneCheck(Check check) => new()
    {
        Method = check.Method,
        DeviceName = check.DeviceName,
        Date = check.Date,
        Location = new Location
        {
            Latitude = check.Location.Latitude,
            Longitude = check.Location.Longitude,
            ApproximateAddress = check.Location.ApproximateAddress,
            IpAddress = check.Location.IpAddress
        },
        PhotoUrl = check.PhotoUrl,
        Descriptor = [.. check.Descriptor],
        Notes = check.Notes
    };
}
