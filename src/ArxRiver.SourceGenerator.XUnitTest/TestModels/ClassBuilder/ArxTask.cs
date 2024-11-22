using ArxRiver.SourceGenerator.Attributes;
using ArxRiver.SourceGenerator.XUnitTest.TestModels.Deconstruct;

namespace ArxRiver.SourceGenerator.XUnitTest.TestModels.ClassBuilder;

[AutoClassBuilder]
public partial class ArxTask
{
    public Guid? TaskId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? EmployeeId { get; set; } = string.Empty;

    public AnimalSpecies Species { get; set; }

    public Animal? AnimalType { get; set; }
}