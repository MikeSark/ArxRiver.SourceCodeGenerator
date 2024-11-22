using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Attributes;
using ArxRiver.SourceGenerator.Configuration;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace ArxRiver.SourceGenerator.Generators.NLogger;

internal sealed class BuildJsonLogger : BaseLogger
{
    internal static void Build(IndentedTextWriter writer, TypeSymbolModel type, ImmutableArray<PropertySymbolModel> properties)
    {
        WriteNullableOption(writer);
        WriteUsings(writer);
        WriteNameSpaceAndTypeLocation(writer, type);

        WriteClassDefinitionBegin(writer, type);

        writer.WriteLines(
            $$"""

              private const string FileTargetName = "jsonFiletarget";

              private Logger? _logger;
              private LoggingConfiguration? _loggingConfiguration;
              private bool _isBuildCalled = false;

              private string _applicationName = AppDomain.CurrentDomain.FriendlyName;
              private LogLevel _minimumLogLevel = LogLevel.Info;
              private LogLevel _maximumLogLevel = LogLevel.Fatal;
              private string _logFileName = string.Empty;
              private bool _concurrentWrites = true;
              private bool _keepFileOpen = false;
              private string _archiveLogFileName = string.Empty;
              private long _archiveAboveSize = 1024 * 1024;
              private int _maxArchiveFiles = 10;
              private ArchiveNumberingMode _archiveNumbering = ArchiveNumberingMode.Rolling;
              private FileArchivePeriod _archiveEvery = FileArchivePeriod.Day;
              private string _archiveDateFormat = "yyyyMMdd";
              private bool _enableArchiveFileCompression = true;
              private bool _archiveOldFileOnStartup = true;

              private readonly FileTarget _fileTarget;
              private  readonly IList<JsonAttribute> _defaultJsonAttributes = new List<JsonAttribute>()
              {
                  new JsonAttribute("timestamp", "${longdate}"),
                  new JsonAttribute("level", "${level}"),
                  new JsonAttribute("logger", "${logger}"),
                  new JsonAttribute("message", "${message}"),
                  new JsonAttribute("exception", "${exception:format=ToString}"),
                  new JsonAttribute("applicationName", "${gdc:item=ApplicationName}")
              };

              """);

        WriteConstructorBegin(writer, type);

        writer.Indent++;
        writer.WriteLine("_fileTarget = new FileTarget(FileTargetName);");
        writer.WriteLine("var jsonLayout = new JsonLayout();");
        writer.WriteLine("foreach (var defaultJsonAttribute in _defaultJsonAttributes) ");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("jsonLayout.Attributes.Add(defaultJsonAttribute);");
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine("_fileTarget.Layout = jsonLayout;");
        writer.Indent--;
        WriteConstructorEnd(writer);


        writer.WriteLines(
            $$"""

              public {{type.Name}}Builder WithApplicationName(string applicationName)
              {
                  _isBuildCalled = false;
                  _applicationName = applicationName;
                  return this;
              }

              public {{type.Name}}Builder WithMinimumLogLevel(LogLevel minimumLogLevel)
              {
                  _isBuildCalled = false;
                  _minimumLogLevel = minimumLogLevel;
                  return this;
              }

              public {{type.Name}}Builder WithMaximumLogLevel(LogLevel maximumLogLevel)
              {
                  _isBuildCalled = false;
                  _maximumLogLevel = maximumLogLevel;
                  return this;
              }

              public {{type.Name}}Builder WithLogFileName(string logFileName)
              {
                  _isBuildCalled = false;
                  _logFileName = logFileName;
                  return this;
              }

              public {{type.Name}}Builder WithConcurrentWrites(bool concurrentWrites)
              {
                  _isBuildCalled = false;
                  _concurrentWrites = concurrentWrites;
                  return this;
              }

              public {{type.Name}}Builder WithKeepFileOpen(bool keepFileOpen)
              {
                  _isBuildCalled = false;
                  _keepFileOpen = keepFileOpen;
                  return this;
              }

              public {{type.Name}}Builder WithArchiveLogFileName(string archiveLogFileName)
              {
                  _isBuildCalled = false;
                  _archiveLogFileName = archiveLogFileName;
                  return this;
              }

              public {{type.Name}}Builder WithArchiveAboveSize(long archiveAboveSize)
              {
                  _isBuildCalled = false;
                  _archiveAboveSize = archiveAboveSize;
                  return this;
              }

              public {{type.Name}}Builder WithMaxArchiveFiles(int maxArchiveFiles)
              {
                  _isBuildCalled = false;
                  _maxArchiveFiles = maxArchiveFiles;
                  return this;
              }

              public {{type.Name}}Builder WithArchiveNumbering(ArchiveNumberingMode archiveNumbering)
              {
                  _isBuildCalled = false;
                  _archiveNumbering = archiveNumbering;
                  return this;
              }

              public {{type.Name}}Builder WithArchiveEvery(FileArchivePeriod archiveEvery)
              {
                  _isBuildCalled = false;
                  _archiveEvery = archiveEvery;
                  return this;
              }

              public {{type.Name}}Builder WithArchiveDateFormat(string archiveDateFormat)
              {
                  _isBuildCalled = false;
                  _archiveDateFormat = archiveDateFormat;
                  return this;
              }

              public {{type.Name}}Builder WithEnableArchiveFileCompression(bool enableArchiveFileCompression)
              {
                  _isBuildCalled = false;
                  _enableArchiveFileCompression = enableArchiveFileCompression;
                  return this;
              }

              public {{type.Name}}Builder WithArchiveOldFileOnStartup(bool archiveOldFileOnStartup)
              {
                  _isBuildCalled = false;
                  _archiveOldFileOnStartup = archiveOldFileOnStartup;
                  return this;
              }

              public {{type.Name}}Builder WithJsonFileLayout(IList<JsonAttribute> jsonAttributes)
              {
                  _isBuildCalled = false;
                  var jsonLayout = new JsonLayout();
                  foreach (var jsonAttribute in jsonAttributes)
                  {
                      jsonLayout.Attributes.Add(jsonAttribute);
                  }
              
                  _fileTarget.Layout = jsonLayout;
                  return this;
              }

              public Logger GetClassLogger()
              {
                  if (!_isBuildCalled)
                      Build();
              
                  _logger = LogManager.GetCurrentClassLogger();
                  return _logger;
              }
              
              public Logger Build()
              {
                  LogManager.ThrowConfigExceptions = true;
                  NLog.GlobalDiagnosticsContext.Set("ApplicationName", _applicationName);
              
                  _loggingConfiguration = new LoggingConfiguration();
                  
                  _fileTarget.FileName = _logFileName;
                  _fileTarget.ConcurrentWrites = _concurrentWrites;
                  _fileTarget.KeepFileOpen = _keepFileOpen;
                  _fileTarget.ArchiveFileName = _archiveLogFileName;
                  _fileTarget.ArchiveAboveSize = _archiveAboveSize;
                  _fileTarget.MaxArchiveFiles = _maxArchiveFiles;
                  _fileTarget.ArchiveNumbering = _archiveNumbering;
                  _fileTarget.ArchiveEvery = _archiveEvery;
                  _fileTarget.ArchiveDateFormat = _archiveDateFormat;
                  _fileTarget.EnableArchiveFileCompression = _enableArchiveFileCompression;
                  _fileTarget.ArchiveOldFileOnStartup = _archiveOldFileOnStartup;
              
                  _loggingConfiguration.AddTarget(_applicationName, _fileTarget);
                  _loggingConfiguration.LoggingRules.Add(new("*", _minimumLogLevel, _maximumLogLevel, _fileTarget));
                  
                  LogManager.Configuration = _loggingConfiguration;
              
                  _logger = LogManager.GetLogger(_applicationName);
                  _isBuildCalled = true;
                  return _logger;
              }

              """);

        WriteClassDefinitionEnd(writer);
    }
}