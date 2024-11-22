using ArxRiver.SourceGenerator.Attributes;
using ArxRiver.SourceGenerator.XUnitTest.TestModels.ClassBuilder;

namespace ArxRiver.SourceGenerator.XUnitTest.TestModels.FluentClass;

[FluentClass]
public partial class JsonLog
{
    public string? Source { get; set; }
    public Guid CorrelationId { get; set; }
    public string Message { get; set; } = string.Empty;
    
    [SkipFluentClass]
    public bool SkippedProperty { get; set; }
}


[FluentClass]
public partial class ClientF
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public AddressF? Address { get; set; }

}

[FluentClass]
public partial class AddressF
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}