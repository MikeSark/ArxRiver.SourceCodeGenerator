using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;

namespace ArxRiver.SourceGenerator.Generators.NLogger;

internal sealed class BuildColoredConsoleLogger : BaseLogger
{
    internal static void Build(IndentedTextWriter writer, TypeSymbolModel type, ImmutableArray<PropertySymbolModel> properties)
    {
        WriteNullableOption(writer);
        WriteUsings(writer);
        WriteNameSpaceAndTypeLocation(writer, type);

        WriteClassDefinitionBegin(writer, type);
        
        writer.WriteLines(
            $$"""
              
              private const string ConsoleTargetName = "coloredconsole";
              private const string LayoutAsString = "${longdate} ${level:uppercase=true} ${logger} - ${message}";
              
              private Logger? _logger;
              private LoggingConfiguration? _loggingConfiguration;
              
              private string _applicationName = AppDomain.CurrentDomain.FriendlyName;
              private LogLevel _minimumLogLevel = LogLevel.Info;
              private LogLevel _maximumLogLevel = LogLevel.Fatal;
              private Layout _consoleLayout = Layout.FromString("${longdate} ${level:uppercase=true} ${logger} - ${message}");
              
              private readonly ColoredConsoleTarget _consoleTarget = new ColoredConsoleTarget(ConsoleTargetName);
              
              private readonly ConsoleRowHighlightingRule[] _defaultColorRules =
              [
                  new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Debug", ForegroundColor = ConsoleOutputColor.Gray },
                  new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Info", ForegroundColor = ConsoleOutputColor.Green },
                  new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Warn", ForegroundColor = ConsoleOutputColor.Yellow },
                  new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Error", ForegroundColor = ConsoleOutputColor.Red },
                  new ConsoleRowHighlightingRule { Condition = "level == LogLevel.Fatal", BackgroundColor = ConsoleOutputColor.Red, ForegroundColor = ConsoleOutputColor.White }
              ];
              
              """);

        WriteConstructorBegin(writer, type);
        
        writer.WriteLines(
            $"""
             foreach (var consoleRowHighlightingRule in _defaultColorRules)
                _consoleTarget.RowHighlightingRules.Add(consoleRowHighlightingRule);
             
             """);

        WriteConstructorEnd(writer);

        writer.WriteLines(
            $$"""
              public {{type.Name}}Builder WithApplicationName(string applicationName)
              {
                  _applicationName = applicationName;
                  return this;
              }
              
              public {{type.Name}}Builder WithMinimumLogLevel(LogLevel minimumLogLevel)
              {
                  _minimumLogLevel = minimumLogLevel;
                  return this;
              }
              
              public {{type.Name}}Builder WithMaximumLogLevel(LogLevel maximumLogLevel)
              {
                  _maximumLogLevel = maximumLogLevel;
                  return this;
              }
              
              public {{type.Name}}Builder WithLayout(string layout)
              {
                  var layoutString = string.IsNullOrWhiteSpace(layout)
                      ? LayoutAsString
                      : layout;
              
                  _consoleLayout = Layout.FromString(layoutString);
                  return this;
              }
              
              public {{type.Name}}Builder WithColorRule(ConsoleRowHighlightingRule rule)
              {
                  var ruleFound = _consoleTarget
                      .RowHighlightingRules
                      .FirstOrDefault(x => x.Condition
                                          .ToString()
                                          .Equals(rule.Condition.ToString(),
                                                  StringComparison.InvariantCultureIgnoreCase));
                  if (null != ruleFound)
                      _consoleTarget.RowHighlightingRules.Remove(ruleFound);
              
                  _consoleTarget.RowHighlightingRules.Add(rule);
                  return this;
              }
              
              
              public Logger Build()
              {
                  LogManager.ThrowConfigExceptions = true;
                  NLog.GlobalDiagnosticsContext.Set("ApplicationName", _applicationName);
              
                  _loggingConfiguration = new LoggingConfiguration();
                  _consoleTarget.Layout = _consoleLayout;
              
                  _loggingConfiguration.AddTarget(_applicationName, _consoleTarget);
                  _loggingConfiguration.LoggingRules.Add(new("*", _minimumLogLevel, _maximumLogLevel, _consoleTarget));
              
                  LogManager.Configuration = _loggingConfiguration;
              
                  _logger = LogManager.GetLogger(_applicationName);
                  return _logger;
              }
              
              """);
        
        WriteClassDefinitionEnd(writer);

    }
}