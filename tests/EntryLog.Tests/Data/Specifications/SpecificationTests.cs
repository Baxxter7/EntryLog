using EntryLog.Data.Specifications;
using EntryLog.Data.Evaluators;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;
using EntryLog.Tests.Helpers;
using FluentAssertions;

namespace EntryLog.Tests.Data.Specifications;

public class SpecificationTests
{
    private sealed class TestSpec : Specification<WorkSession> { }

    [Fact]
    public void DefaultExpression_MatchesAllItems()
    {
        var spec = new TestSpec();
        var sessions = new List<WorkSession>
        {
            new WorkSessionBuilder().WithEmployeeId(1).Build(),
            new WorkSessionBuilder().WithEmployeeId(2).Build()
        };

        var result = sessions.AsQueryable().Where(spec.Expression).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public void AndAlso_FiltersCorrectly()
    {
        var spec = new TestSpec();
        spec.AndAlso(x => x.EmployeeId == 1);

        var sessions = new List<WorkSession>
        {
            new WorkSessionBuilder().WithEmployeeId(1).Build(),
            new WorkSessionBuilder().WithEmployeeId(2).Build()
        };

        var result = sessions.AsQueryable().Where(spec.Expression).ToList();

        result.Should().HaveCount(1);
        result[0].EmployeeId.Should().Be(1);
    }

    [Fact]
    public void AndAlso_MultipleFilters_CombinesWithAnd()
    {
        var spec = new TestSpec();
        spec.AndAlso(x => x.EmployeeId == 1);
        spec.AndAlso(x => x.Status == SessionStatus.InProgress);

        var sessions = new List<WorkSession>
        {
            new WorkSessionBuilder().WithEmployeeId(1).WithStatus(SessionStatus.InProgress).Build(),
            new WorkSessionBuilder().WithEmployeeId(1).WithStatus(SessionStatus.Completed).Build(),
            new WorkSessionBuilder().WithEmployeeId(2).WithStatus(SessionStatus.InProgress).Build()
        };

        var result = sessions.AsQueryable().Where(spec.Expression).ToList();

        result.Should().HaveCount(1);
    }

    [Fact]
    public void ApplyPaging_SetsTakeAndSkip()
    {
        var spec = new TestSpec();

        spec.ApplyPaging(10, 20);

        spec.Take.Should().Be(10);
        spec.Skip.Should().Be(20);
        spec.IsPagingEnabled.Should().BeTrue();
    }

    [Fact]
    public void AndOrderBy_SetsAscendingOrder()
    {
        var spec = new TestSpec();

        spec.AndOrderBy(x => x.EmployeeId);

        spec.OrderBy.Should().NotBeNull();
        spec.OrderByDescending.Should().BeNull();
    }

    [Fact]
    public void AndOrderByDescending_SetsDescendingOrder()
    {
        var spec = new TestSpec();

        spec.AndOrderByDescending(x => x.EmployeeId);

        spec.OrderByDescending.Should().NotBeNull();
        spec.OrderBy.Should().BeNull();
    }

    [Fact]
    public void ApplyPaging_NegativeTake_Throws()
    {
        var spec = new TestSpec();

        var act = () => spec.ApplyPaging(-1, 0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ApplyPaging_NegativeSkip_Throws()
    {
        var spec = new TestSpec();

        var act = () => spec.ApplyPaging(10, -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SpecificationEvaluator_AppliesFilterSortAndPaging()
    {
        var spec = new TestSpec();
        spec.AndAlso(x => x.EmployeeId >= 1);
        spec.AndOrderByDescending(x => x.EmployeeId);
        spec.ApplyPaging(2, 1);

        var sessions = new List<WorkSession>
        {
            new WorkSessionBuilder().WithEmployeeId(1).Build(),
            new WorkSessionBuilder().WithEmployeeId(2).Build(),
            new WorkSessionBuilder().WithEmployeeId(3).Build()
        }.AsQueryable();

        var result = SpecificationEvaluator<WorkSession>.GetQuery(sessions, spec).ToList();

        result.Select(x => x.EmployeeId).Should().Equal(2, 1);
    }
}
