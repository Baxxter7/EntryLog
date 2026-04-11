using EntryLog.Entities.POCOEntities;

namespace EntryLog.Tests.Helpers;

internal sealed class EmployeeBuilder
{
    private int _code = 1001;
    private string _fullName = "John Doe";
    private int _positionId = 1;
    private DateTime _dateOfBirthday = new(1990, 1, 1);
    private string _townName = "Test Town";
    private Position _position = new()
    {
        Id = 1,
        Name = "Developer",
        Description = "Dev"
    };

    public EmployeeBuilder WithCode(int code) { _code = code; return this; }
    public EmployeeBuilder WithFullName(string name) { _fullName = name; return this; }
    public EmployeeBuilder WithPositionId(int positionId) { _positionId = positionId; return this; }
    public EmployeeBuilder WithDateOfBirthday(DateTime dateOfBirthday) { _dateOfBirthday = dateOfBirthday; return this; }
    public EmployeeBuilder WithTownName(string townName) { _townName = townName; return this; }
    public EmployeeBuilder WithPosition(Position position)
    {
        _position = position;
        _positionId = position.Id;
        return this;
    }

    public Employee Build()
    {
        return new Employee
        {
            Code = _code,
            FullName = _fullName,
            PositionId = _positionId,
            DateofBirthday = _dateOfBirthday,
            TownName = _townName,
            Position = new Position
            {
                Id = _positionId,
                Name = _position.Name,
                Description = _position.Description
            }
        };
    }
}
