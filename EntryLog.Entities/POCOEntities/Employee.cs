namespace EntryLog.Entities.POCOEntities;

public class Employee
{
    public int Code { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PositionId { get; set; } = string.Empty;
    public DateTime DateofBirthday { get; set; }
    public string BranchOffice { get; set; } = string.Empty;
    public string TownName { get; set; } = string.Empty;
    public string CostCenter { get; set; } = string.Empty;
    public Position Position { get; set; } = new Position();

}
