namespace EntryLog.Entities.POCOEntities;

public class Employee
{
    public int Code { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int PositionId { get; set; }
    public DateTime DateofBirthday { get; set; }
    public string TownName { get; set; } = string.Empty;
    public Position Position { get; set; } = new Position();

}
