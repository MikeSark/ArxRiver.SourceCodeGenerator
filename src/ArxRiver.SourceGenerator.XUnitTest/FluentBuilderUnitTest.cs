using ArxRiver.SourceGenerator.XUnitTest.TestModels.ClassBuilder;
using FluentAssertions;

namespace ArxRiver.SourceGenerator.XUnitTest;

public class FluentBuilderUnitTest
{
    [Fact]
    public void fluent_builder_class_creation_test()
    {
        var addressBuilder = AddressBuilder.Create()
            .WithCity("Burbank")
            .WithState("CA")
            .WithStreet("874 Glenoaks blvd")
            .WithZipCode("91502");

        var address = addressBuilder.Build();

        address.City.Should().Be("Burbank");
        address.State.Should().Be("CA");
        address.Street.Should().Be("874 Glenoaks blvd");
        address.ZipCode.Should().Be("91502");
    }


    [Fact]
    public void fluent_builder_class_with_object_test()
    {
        var client= ClientBuilder.Create()
            .WithValidationRule(c => c is { Address: not null, Note: not null })
            .WithId(1)
            .WithAddress(a => a
                .WithCity("Burbank")
                .WithStreet("780 GlenOaks Blvd")
                .WithZipCode("54695")
                .WithValidationRule(a => !string.IsNullOrWhiteSpace(a.City) && !string.IsNullOrWhiteSpace(a.Street))
            )
            .WithNote(n => n
                .WithTitle("single Note")
                .WithContent("single note content")
                .WithCreationDate(DateTime.Now)
                .WithId(500)
                .WithValidationRule(n => n.Id > 0 && !string.IsNullOrWhiteSpace(n.Title))
            )
            .WithEmail("")
            .WithPhoneNumber("818-747-6985")
            .WithNotes([
                new Note() { Title = "", Content = "", CreationDate = DateTime.Now, Id = 2500 },
                new Note() { Title = "", Content = "", CreationDate = DateTime.Now, Id = 3000 },
                new Note() { Title = "", Content = "", CreationDate = DateTime.Now, Id = 4500 }
            ])
            .Build();

        client.Id.Should().Be(1);
        client.Address.City.Should().Be("Burbank");
        client.Note.Title.Should().Be("single Note");
        client.Notes.Count.Should().Be(3);
        client.Notes.First().Title.Should().Be("");
        client.Notes.Last().Title.Should().Be("");
        
    }
}