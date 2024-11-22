using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace ArxRiver.SourceGenerator.Configuration;

public sealed class GeneratorConstants
{
    public const string DeconstructAttributeNmae = "DeconstructAttribute";
    public const string IncludeInDeconstructAttributeName = "IncludeInDeconstructAttribute";
    public const string DeconstructAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.DeconstructAttribute";
    public const string IncludeInDeconstructAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.IncludeInDeconstructAttribute";
    
    public const string ExcludeAttributeName = "ExcludeAttribute";
    public const string ExcludeAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.ExcludeAttribute";

    public const string CloneableAttributeName = "CloneableAttribute";
    public const string CloneAttributeName = "CloneAttribute";
    public const string DeepCloneAttributeName = "DeepCloneAttribute";
    public const string SkipCloneableAttributeName = "SkipCloneableAttribute";
    
    public const string CloneableAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.CloneableAttribute";
    public const string CloneAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.CloneAttribute";
    public const string DeepCloneAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.DeepCloneAttribute";
    public const string SkipCloneableAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.SkipCloneableAttribute";

    public const string AutoClassBuilderAttributeName = "AutoClassBuilderAttribute";
    public const string AutoClassBuilderAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.AutoClassBuilderAttribute";
    public const string SkipClassBuilderAttributeName = "SkipClassBuilderAttribute";
    public const string SkipClassBuilderAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.SkipClassBuilderAttribute";

    public const string FluentClassBuilderAttributeName = "FluentClassAttribute";
    public const string FluentClassBuilderAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.FluentClassAttribute";
    public const string SkipFluentClassBuilderAttributeName = "SkipFluentClassAttribute";
    public const string SkipFluentClassBuilderAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.SkipFluentClassAttribute";
    

    public const string AutoTaskManagerAttributeName = "AutoTaskManagerAttribute";
    public const string AutoTaskManagerAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.AutoTaskManagerAttribute";

    public const string RetryResiliencyAttributeName = "RetryResiliencyAttribute";
    public const string RetryResiliencyAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.RetryResiliencyAttribute";


    public const string PsLoggerAttributeName = "PsLoggerAttribute";
    public const string PsLoggerAttributeFullPath = "ArxRiver.SourceGenerator.Attributes.PsLoggerAttribute";





    public static readonly DiagnosticDescriptor MissingPartialModifierRule = new DiagnosticDescriptor(
        id: "GEN001",
        title: "Class should be partial",
        messageFormat: $"Class '{0}' must be partial to work with this Clonable generator.",
        category: "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

}