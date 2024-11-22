using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArxRiver.SourceGenerator.Attributes;

namespace ArxRiver.SourceGenerator.XUnitTest.TestModels.Cloning;

[Cloneable]
internal partial class Student
{
    public Student() { }

    public Student(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public string? Name { get; set; }
    public int Age { get; set; }

    public List<string>? CourseNames { get; set; }
    
    public List<Course>? Courses { get; set; }
}