using ArxRiver.SourceGenerator.XUnitTest.TestModels.Deconstruct;
using FluentAssertions;

namespace ArxRiver.SourceGenerator.XUnitTest;

public class DeconstructClassExtensionUnitTest
{
    [Fact]
    public void person_class_has_deconstruct()
    {

        var personObject = new Person()
        {
            FirstName = "Visual",
            LastName = "Studio",
            Age = 27
        };
        
        var (firstName, lastName) = personObject;
        
        firstName.Should().Be("Visual", "Deconstruct did not generate a field for first name");
        lastName.Should().Be("Studio", "Deconstruct did not generate a field for last name");
        
    }
    
    // write a xunit method for Animal class
    [Fact]
    public void animal_class_has_deconstruct()  
    {
        var animalObject = new Animal()
        {
            Name = "Fluffy",
            Father = new Animal()
            {
                Name = "Fido"
            },
            Mother = new Animal()
            {
                Name = "Dorothy"
            },
            Sound = "Meow",
            Age = 5
        };

        var (name, sound, father, mother) = animalObject;

        name.Should().Be("Fluffy", "Deconstruct did not generate a field for name");
        sound.Should().Be("Meow", "Deconstruct did not generate a field for sound");
        father.Should().NotBeNull("Deconstruct did not generate a field for sound");
        father!.Name.Should().Be("Fido", "Deconstruct did not generate a field for father");
        mother.Should().NotBeNull("Deconstruct did not generate a field for sound");
        mother!.Name.Should().Be("Dorothy", "Deconstruct did not generate a field for mother");
    }
}