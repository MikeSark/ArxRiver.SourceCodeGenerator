using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using ArxRiver.SourceGenerator.Configuration;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ArxRiver.SourceGenerator.Generators.Cloneable;

[Generator]
public class CloneableGenerator : IIncrementalGenerator
{
    private const string ClonableName = "Clonable";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var typeProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is TypeDeclarationSyntax,
                static (context, token) =>
                    {
                        var symbol = context.SemanticModel.GetDeclaredSymbol((TypeDeclarationSyntax)context.Node, token);
                        if (symbol is null)
                        {
                            return null;
                        }
                        else if (symbol is INamedTypeSymbol namedTypeSymbol &&
                                 namedTypeSymbol
                                     .GetAttributes()
                                     .Any(a => a.AttributeClass?.ToDisplayString() == GeneratorConstants.CloneableAttributeFullPath))
                        {
                            var accessibleProperties = symbol.GetAccessibleProperties();
                            if (accessibleProperties.IsEmpty ||
                                symbol.GetMembers().OfType<IMethodSymbol>().Any(
                                    m => m.Name == ClonableName &&
                                         !m.IsStatic && m.Parameters.Length == accessibleProperties.Length &&
                                         m.Parameters.All(p => p.RefKind == RefKind.Out)))
                            {
                                return null;
                            }

                            return new TypeSymbolModel(symbol, symbol.ContainingNamespace.ToString(),
                                                       symbol.Name,
                                                       symbol.GetGenericParameters(),
                                                       symbol.GetFullyQualifiedName(),
                                                       symbol.GetConstraints(),
                                                       symbol.IsValueType,
                                                       symbol.IsPartial(),
                                                       accessibleProperties);
                        }

                        return null;
                    })
            .Where(static m => m is not null);

        var excludedTypesHavingCloneable = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is MethodDeclarationSyntax methodNode &&
                                    methodNode.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                                    methodNode.Identifier.ValueText == ClonableName &&
                                    methodNode.ParameterList.Parameters.Count > 1 &&
                                    methodNode.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword) &&
                                    methodNode.ParameterList.Parameters.Count(parameter => parameter.Modifiers.Any(SyntaxKind.OutKeyword)) ==
                                    methodNode.ParameterList.Parameters.Count - 1,
                static (context, token) =>
                    {
                        var methodSymbol = context.SemanticModel.GetDeclaredSymbol((MethodDeclarationSyntax)context.Node, token);
                        if (methodSymbol?.Parameters[0].Type is not INamedTypeSymbol type)
                        {
                            return null;
                        }

                        var accessibleProperties = type.GetAccessibleProperties();
                        if (accessibleProperties.Length == methodSymbol.Parameters.Length - 1)
                        {
                            return new TypeSymbolModel(type, type.ContainingNamespace.ToString(),
                                                       type.Name,
                                                       type.GetGenericParameters(),
                                                       type.GetFullyQualifiedName(),
                                                       type.GetConstraints(),
                                                       type.IsValueType,
                                                       type.IsPartial(),
                                                       accessibleProperties);
                        }

                        return null;
                    })
            .Where(static m => m is not null);

        context.RegisterSourceOutput(typeProvider.Collect().Combine(excludedTypesHavingCloneable.Collect()),
                                     (spc, source) =>
                                         {
                                             var (compilation, _) = source;
                                             foreach (var symbolModel in compilation)
                                             {
                                                 if (symbolModel == null) continue;
                                                 if (symbolModel.IsPartial) continue;

                                                 var diagnostic = Diagnostic.Create(
                                                     GeneratorConstants.MissingPartialModifierRule,
                                                     Location.None,
                                                     symbolModel.Name);

                                                 spc.ReportDiagnostic(diagnostic);
                                             }

                                             CreateOutput(source.Left!, source.Right!, spc);
                                         });
    }

    private void CreateOutput(ImmutableArray<TypeSymbolModel> sourceLeft, ImmutableArray<TypeSymbolModel> sourceRight, SourceProductionContext context)
    {
        if (sourceLeft.Length > 0)
        {
            using var writer = new StringWriter();
            using var indentWriter = new IndentedTextWriter(writer, "\t");

            foreach (var type in sourceLeft.Distinct())
            {
                var accessibleProperties = type.AccessibleProperties;

                if (!sourceRight.Contains(type))
                {
                    ClonableBuilder.Build(indentWriter, type, accessibleProperties);
                    context.AddSource($"ArxRiver.Cloneable.{type.Name.ToSafeClassName()}.g.cs", SourceText.From(writer.ToString(), Encoding.UTF8));
                }

                writer.GetStringBuilder().Clear();
            }
        }
    }
}