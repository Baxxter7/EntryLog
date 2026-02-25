using EntryLog.Business.DTOs;
using EntryLog.Business.Utils;
using EntryLog.Entities.POCOEntities;
using System.Globalization;

namespace EntryLog.Business.Mappers;

internal class FaceIdMapper
{
    public static EmployeeFaceIdDto MapToEmployeeFaceIdDto(FaceID faceId, String base64Image)
    => new(base64Image,
        TimeFunctions.GetCentralAmericaStandardTime(faceId.RegisterDate)
            .ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture),
        faceId.Active
       );

    public static EmployeeFaceIdDto Empty() => new("", "", false);
}
