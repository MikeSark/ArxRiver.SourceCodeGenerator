using ArxRiver.SourceGenerator.Attributes;

namespace ArxRiver.SourceGenerator.XUnitTest.TestModels.Deconstruct;

[Deconstruct]
public partial class Animal
{
    [IncludeInDeconstruct]
    public string? Name { get; set; }

    public int Age { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }

    [IncludeInDeconstruct]
    public string? Sound { get; set; }

    [IncludeInDeconstruct]
    public Animal? Father { get; set; }

    [IncludeInDeconstruct]
    public Animal? Mother { get; set; }
}