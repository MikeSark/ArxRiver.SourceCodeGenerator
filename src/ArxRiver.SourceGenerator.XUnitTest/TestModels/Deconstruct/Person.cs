using ArxRiver.SourceGenerator.Attributes;

namespace ArxRiver.SourceGenerator.XUnitTest.TestModels.Deconstruct;

[Deconstruct]
public class Person
{

    [IncludeInDeconstruct]
    public string? FirstName { get; set; }

    [IncludeInDeconstruct]
    public string? LastName { get; set; }


    public int Age { get; set; }
}