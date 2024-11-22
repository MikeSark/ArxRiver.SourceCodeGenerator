using ArxRiver.SourceGenerator.Attributes;

namespace ArxRiver.SourceGenerator.XUnitTest.TestModels.ClassBuilder;

[AutoClassBuilder]
public class Client
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public Address? Address { get; set; }

    public Note? Note { get; set; }

    public List<Note>? Notes { get; set; }
}