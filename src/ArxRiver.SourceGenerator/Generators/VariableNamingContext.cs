using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ArxRiver.SourceGenerator;

internal sealed class VariableNamingContext
{
    private readonly ImmutableArray<string> _parameterNames;
    private readonly Dictionary<string, string> _variables = new();

    internal VariableNamingContext(IMethodSymbol method) :
        this(method.Parameters) { }

    internal VariableNamingContext(ImmutableArray<IParameterSymbol> parameters) :
        this(parameters.Select(s => s.Name).ToImmutableArray()) { }

    internal VariableNamingContext(ImmutableArray<string> parameterNames) =>
        _parameterNames = parameterNames;

    internal string this[string variableName]
    {
        get
        {
            if (_variables.TryGetValue(variableName, out var item)) return item;

            var uniqueName = variableName;
            var id = 1;

            // ReSharper disable once AccessToModifiedClosure
            while (_parameterNames.Any(s => s == uniqueName) || _variables.ContainsKey(uniqueName))
            {
                uniqueName = $"{variableName}{id++}";
            }

            _variables.Add(variableName, uniqueName);

            return _variables[variableName];
        }
    }
}