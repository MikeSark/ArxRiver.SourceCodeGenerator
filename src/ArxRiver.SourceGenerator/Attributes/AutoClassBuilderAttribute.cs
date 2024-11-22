namespace ArxRiver.SourceGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
                AllowMultiple = false, Inherited = false)]
public sealed class AutoClassBuilderAttribute : Attribute { }