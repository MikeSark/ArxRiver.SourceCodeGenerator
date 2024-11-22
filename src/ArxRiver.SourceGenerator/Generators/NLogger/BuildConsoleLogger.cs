using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;

namespace ArxRiver.SourceGenerator.Generators.NLogger;

internal class BuildConsoleLogger : BaseLogger
{
    internal static void Build(IndentedTextWriter writer, TypeSymbolModel type, ImmutableArray<PropertySymbolModel> properties)
    {
        WriteNullableOption(writer);
        WriteUsings(writer);
        WriteNameSpaceAndTypeLocation(writer, type);

        WriteClassDefinitionBegin(writer, type);

        writer.WriteLines(
            $$"""

              private const string ConsoleTargetName = "console";
              private const string LayoutAsString = "${longdate} ${level:uppercase=true} ${logger} - ${message}";

              private Logger? _logger;
              private LoggingConfiguration? _loggingConfiguration;

              private string _applicationName = AppDomain.CurrentDomain.FriendlyName;
              private LogLevel _minimumLogLevel = LogLevel.Info;
              private LogLevel _maximumLogLevel = LogLevel.Fatal;

              private Layout _consoleLayout = Layout.FromString(LayoutAsString);
              private readonly ConsoleTarget _consoleTarget = new ConsoleTarget(ConsoleTargetName);

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