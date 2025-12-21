using EntryLog.Business.DTOs;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Business.Mappers;

internal static class WorkSessionMapper
{
    public static GetWorkSessionDto MapToGetWorkSessionDto(WorkSession session)
    {
        return new GetWorkSessionDto(
            session.Id.ToString(),
            session.EmployeeId,
            new GetCheckDto(
                session.CheckIn.Method,
                session.CheckIn.DeviceName,
                session.CheckIn.Date,
                new GetLocationDto(
                    session.CheckIn.Location.Latitude,
                    session.CheckIn.Location.Longitude,
                    session.CheckIn.Location.IpAddress
                ),
                session.CheckIn.PhotoUrl,
                session.CheckIn.Notes),
                session.CheckOut != null ? new GetCheckDto(
                session.CheckOut.Method,
                session.CheckOut.DeviceName,
                session.CheckOut.Date,
                new GetLocationDto(
                    session.CheckOut.Location.Latitude,
                    session.CheckOut.Location.Longitude,
                    session.CheckOut.Location.IpAddress
                ),
                session.CheckOut.PhotoUrl,
                session.CheckOut.Notes) : null,
                session.TotalWorked,
                session.Status.ToString()
            );
    }
}
