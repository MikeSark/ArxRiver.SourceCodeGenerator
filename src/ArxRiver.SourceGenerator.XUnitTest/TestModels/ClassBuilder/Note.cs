﻿using ArxRiver.SourceGenerator.Attributes;

namespace ArxRiver.SourceGenerator.XUnitTest.TestModels.ClassBuilder;

[AutoClassBuilder]
public class Note
{
    public int Id { get; set; }
    public string? Title { get; set; } = null;
    public string? Content { get; set; } = null;
    public DateTime CreationDate { get; set; } = DateTime.Now;
}