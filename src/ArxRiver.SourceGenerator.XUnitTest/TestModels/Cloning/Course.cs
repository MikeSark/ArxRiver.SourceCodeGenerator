using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArxRiver.SourceGenerator.Attributes;

namespace ArxRiver.SourceGenerator.XUnitTest.TestModels;

[Cloneable]
internal partial class Course
{
    public int CourseId { get; set; }
    
    public string? CourseName { get; set; }
    
}
