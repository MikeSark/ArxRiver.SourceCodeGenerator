using ArxRiver.SourceGenerator.XUnitTest.TestModels.ClassBuilder;
using FluentAssertions;

namespace ArxRiver.SourceGenerator.XUnitTest;

public class ClassBuilderUnitTest
{
    [Fact]
    public void arx_task_class_has_a_class_builder()
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
}