namespace EntryLog.Entities.POCOEntities;

public class Employee
{
    public int Code { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int OrganizationID { get; set; }
    public string BranchOffice { get; set; } = string.Empty;
    public string TownName { get; set; } = string.Empty;
    public string CostCenter { get; set; } = string.Empty;
}
