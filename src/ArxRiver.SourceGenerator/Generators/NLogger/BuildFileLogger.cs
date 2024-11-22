using System.CodeDom.Compiler;
using System.Collections.Immutable;
using ArxRiver.SourceGenerator.Extensions;
using ArxRiver.SourceGenerator.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArxRiver.SourceGenerator.Generators.NLogger;

internal sealed class BuildFileLogger : BaseLogger
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
              private readonly string _defaultFileLayout = "${longdate}|${level:uppercase=true}|${logger}|${message}${exception:format=ToString}";
              
              """);

        WriteConstructorBegin(writer, type);
        
        writer.WriteLine(
            $$"""
              _fileTarget = new FileTarget(FileTargetName);
              _fileTarget.Layout = _defaultFileLayout;
              
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
              
              public {{type.Name}}Builder WithLogFileName(string logFileName)
              {
                  _logFileName = logFileName;
                  return this;
              }
              
              public {{type.Name}}Builder WithConcurrentWrites(bool concurrentWrites)
              {
                  _concurrentWrites = concurrentWrites;
                  return this;
              }
              
              public {{type.Name}}Builder WithKeepFileOpen(bool keepFileOpen)
              {
                  _keepFileOpen = keepFileOpen;
                  return this;
              }
              
              public {{type.Name}}Builder WithArchiveLogFileName(string archiveLogFileName)
              {
                  _archiveLogFileName = archiveLogFileName;
                  return this;
              }
              
              public {{type.Name}}Builder WithArchiveAboveSize(long archiveAboveSize)
              {
                  _archiveAboveSize = archiveAboveSize;
                  return this;
              }
              
              public {{type.Name}}Builder WithMaxArchiveFiles(int maxArchiveFiles)
              {
                  _maxArchiveFiles = maxArchiveFiles;
                  return this;
              }
              
              public {{type.Name}}Builder WithArchiveNumbering(ArchiveNumberingMode archiveNumbering)
              {
                  _archiveNumbering = archiveNumbering;
                  return this;
              }
              
              public {{type.Name}}Builder WithArchiveEvery(FileArchivePeriod archiveEvery)
              {
                  _archiveEvery = archiveEvery;
                  return this;
              }
              
              public {{type.Name}}Builder WithArchiveDateFormat(string archiveDateFormat)
              {
                  _archiveDateFormat = archiveDateFormat;
                  return this;
              }
              
              public {{type.Name}}Builder WithEnableArchiveFileCompression(bool enableArchiveFileCompression)
              {
                  _enableArchiveFileCompression = enableArchiveFileCompression;
                  return this;
              }
              
              public {{type.Name}}Builder WithArchiveOldFileOnStartup(bool archiveOldFileOnStartup)
              {
                  _archiveOldFileOnStartup = archiveOldFileOnStartup;
                  return this;
              }
              
              
              public {{type.Name}}Builder WithFileLayout(string layoutString)
              {
                  var layoutChecked = string.IsNullOrWhiteSpace(layoutString) 
                    ? _defaultFileLayout
                    : layoutString;
                
                  _fileTarget.Layout = Layout.FromString(layoutChecked);
                  return this;
              }
              
              public Logger Build()
              {
                  LogManager.ThrowConfigExceptions = true;
                  NLog.GlobalDiagnosticsContext.Set("ApplicationName", "PS LOG");
              
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
                  return _logger;
              }
              
              """);
        
        WriteClassDefinitionEnd(writer);
    }
}