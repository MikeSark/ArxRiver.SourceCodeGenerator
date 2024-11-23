using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

public class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Default);

    [Solution] readonly Solution Solution;
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly string Configuration = IsLocalBuild ? "Debug" : "Release";

    private readonly string PackageName = "ArxRiver.SourceGenerator";
    private readonly string SourceGeneratorPath = "ArxRiver.SourceGenerators";
    private readonly string SourceGeneratorTestPath = "ArxRiver.SourceGenerator.XUnitTest";

    AbsolutePath SourceDirectory => RootDirectory / "Src";
    AbsolutePath OutPutDirectory => RootDirectory / "Output";
    AbsolutePath PublishDirectory => RootDirectory / "Deploy";
    AbsolutePath PackageDirectory => RootDirectory / "PSPackages";
    AbsolutePath SourceGeneratorBinDirectory => OutPutDirectory / Configuration;

    //AbsolutePath SourceGeneratorOutPutDirectory => OutPutDirectory / "Sourcegenerator";
    AbsolutePath SourceGeneratorTestOutPutDirectory => OutPutDirectory / "SourcegeneratorUniTest";

    //AbsolutePath NugetPackageFiles => SourceGeneratorOutPutDirectory / $"{PackageName}*.*nupkg";

    Target Default => d => d.DependsOn(Finalize);


    Target Finalize => d => d
        .DependsOn(RunTest)
        .Executes(() =>
        {
            Log.Information("Target: Finalize");

            // cleanup the temporary directories
            Directory.Delete(SourceGeneratorTestOutPutDirectory, true);
            Directory.Delete(PackageDirectory, true);
        });


    Target RunTest => d => d
        .DependsOn(BuildTestProject)
        .Executes(() =>
        {
            Log.Information("Target: RunTest");

            DotNetTest(s => s.SetProjectFile(Solution.GetProject(SourceGeneratorTestPath))
                           .SetConfiguration(Configuration)
                           .SetResultsDirectory(RootDirectory / "test-results")
                           .SetVerbosity(DotNetVerbosity.normal)
            );
        });


    Target BuildTestProject => d => d
        .DependsOn(UpdatePackageReference)
        .Executes(() =>
        {
            Log.Information("Target: BuildTestProject");

            var project = Solution.GetProject(SourceGeneratorTestPath);
            if (null == project)
            {
                throw new Exception("Unable to identify the test project. Build target UpdatePackageReference, threw an exception.");
            }

            var sources = GetNuGetSourcesFromDotNet();
            sources.Add(PackageDirectory);

            DotNetRestore(s => s.SetProjectFile(project.Path)
                              .AddSources(sources)
                              .SetProperty("RestorePackagesPath", PackageDirectory));

            ShowPackageNames();


            DotNetBuild(s => s
                            .SetProjectFile(Solution.GetProject(SourceGeneratorTestPath))
                            .SetConfiguration(Configuration)
                            .SetOutputDirectory(SourceGeneratorTestOutPutDirectory));
        });


    Target UpdatePackageReference => d => d
        .DependsOn(BuildSourcegenerator)
        .Executes(() =>
        {
            Log.Information("Target: UpdatePackageReference");

            var project = Solution.GetProject(SourceGeneratorTestPath);
            if (null == project)
            {
                throw new Exception("Unable to identify the test project. Build target UpdatePackageReference, threw an exception.");
            }

            var packageVersion = GetPackageVersionNumber();
            if (string.IsNullOrEmpty(packageVersion))
            {
                throw new Exception("package Version number not found. Build target UpdatePackageReference, threw an exception.");
            }

            DotNet($"add {project.Path} package {PackageName}  -v {packageVersion}  -s {PackageDirectory}");
        });

    Target BuildSourcegenerator => d => d
        .DependsOn(Clean)
        .Executes(() =>
        {
            Log.Information("Target: BuildSourcegenerator");

            var project = Solution.GetProject(SourceGeneratorPath);
            if (null == project)
            {
                throw new Exception("Unable to identify the source generator project. Build target UpdatePackageReference, threw an exception.");
            }

            var targetFramework = GetTargetFrameworkFromCsproj(project.Path);
            var outputDirectory = SourceGeneratorBinDirectory / targetFramework;

            // Restore and build first project, the source generator
            DotNetRestore(s => s.SetProjectFile(project.Path)
                              .SetProperty("RestorePackagesPath", PackageDirectory));

            ShowPackageNames();

            DotNetBuild(s => s
                            .SetProjectFile(Solution.GetProject(SourceGeneratorPath))
                            .SetConfiguration(Configuration)
                            .SetOutputDirectory(outputDirectory));


            DotNetPack(s => s
                           .SetProject(Solution) // Replace 'Solution' with a specific project file if needed
                           .SetConfiguration(Configuration)
                           .SetOutputDirectory(PackageDirectory)
                           .EnableNoRestore()); // Skip restore if already done
        });


    Target Clean =>
        x => x
            .DependsOn(Announce)
            .DependsOn(ModifyVersionNumber)
            .Executes(() =>
            {
                Log.Information("Target: Clean");

                Log.Information("Clearing *.bin  and *.obj folders");
                SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());

                // Log.Information($"Deleting {SourceGeneratorOutPutDirectory}");
                // SourceGeneratorOutPutDirectory.DeleteDirectory();

                Log.Information($"Deleting {SourceGeneratorTestOutPutDirectory}");
                SourceGeneratorTestOutPutDirectory.DeleteDirectory();

                Log.Information($"Deleting {OutPutDirectory}");
                OutPutDirectory.DeleteDirectory();

                Log.Information($"Deleting {PublishDirectory}");
                PublishDirectory.DeleteDirectory();
            });


    Target Announce =>
        d => d
            .Executes(() =>
            {
                Log.Information("Target: Announce");

                Log.Information($"Configuration: {Configuration}");
                Log.Information($"RootDirectory : {RootDirectory}");
                Log.Information($"SourceDirectory : {SourceDirectory}");
                Log.Information($"OutPutDirectory : {OutPutDirectory}");
                Log.Information($"PublishDirectory : {PublishDirectory}");
                Log.Information($"PackageDirectory : {PackageDirectory}");
                Log.Information($"SourceGeneratorPath : {SourceGeneratorPath}");
                Log.Information($"SourceGeneratorTestPath : {SourceGeneratorTestPath}");
                //Log.Information($"NugetPackageFiles : {NugetPackageFiles}");
                Log.Information($"SourceGeneratorBinDirectory : {SourceGeneratorBinDirectory}");
            });


    Target ModifyVersionNumber =>
        d => d
            .Executes(() =>
            {
                Log.Information("Target: ModifyVersionNumber");
                var buildPropFileName = GetBuildPropVersionXmlElement(out var xmlDocument, out var versionNode);
                if (versionNode != null)
                {
                    Log.Information($"Current Version: {versionNode.Value}");
                    versionNode.Value = ValidateAndIncreaseVersionNumber(versionNode.Value);
                    Log.Information($"updated Version: {versionNode.Value}");
                }
                else
                {
                    var root = xmlDocument.Root;
                    if (null != root)
                    {
                        Log.Warning("Version node not found.");
                        versionNode = new XElement("Version", "1.0.0"); // Set the initial version value
                        root.Add(versionNode);
                        xmlDocument.Save(buildPropFileName);
                        Log.Information("Version node added with value: 1.0.0");
                    }
                }

                xmlDocument.Save(buildPropFileName);
            });

    private List<string> GetNuGetSourcesFromDotNet()
    {
        var output = DotNetTasks.DotNet("nuget list source", logOutput: false);
        var lines = output.Select(x => x.Text).ToList();

        Log.Debug(string.Join(", ", lines));
        return lines
            .Where(line => line.StartsWith("   ") && line.Contains("http"))
            .Select(line => line.Trim())
            .ToList();
    }


    private string GetTargetFrameworkFromCsproj(AbsolutePath projectFilePath)
    {
        if (!File.Exists(projectFilePath))
        {
            Log.Warning($"The file '{projectFilePath}' does not exist.");
            return null;
        }

        var document = XDocument.Load(projectFilePath);
        return document
            .Descendants("TargetFramework")
            .FirstOrDefault()
            ?.Value;
    }

    private void ShowPackageNames()
    {
        if (!Directory.Exists(PackageDirectory))
        {
            Log.Warning($"Directory '{PackageDirectory}' does not exist.");
            return;
        }

        var subfolders = Directory.GetDirectories(PackageDirectory);
        if (subfolders.Length == 0)
        {
            Log.Information($"No subfolders found in '{PackageDirectory}'.");
        }
        else
        {
            Log.Information($"Subfolders in '{PackageDirectory}':");
            foreach (var subfolder in subfolders)
            {
                Log.Information(Path.GetFileName(subfolder)); // Output only the folder name
            }
        }
    }


    private string GetPackageVersionNumber()
    {
        GetBuildPropVersionXmlElement(out var xmlDocument, out var versionNode);
        if (versionNode != null)
        {
            return versionNode.Value;
        }

        return string.Empty;
    }


    private AbsolutePath GetBuildPropVersionXmlElement(out XDocument xmlDocument, out XElement versionNode)
    {
        var buildPropFileName = SourceDirectory / "Directory.Build.props";
        xmlDocument = XDocument.Load(buildPropFileName);
        versionNode = xmlDocument.Descendants("Version").FirstOrDefault();
        return buildPropFileName;
    }


    private string ValidateAndIncreaseVersionNumber(string versionValue)
    {
        var versionParts = versionValue.Split('.');
        if (versionParts.Length == 0)
            return "1.0.0";

        if (versionParts.Length != 3 && versionParts.Length == 2)
            versionParts = new[] { "1", versionParts[0], versionParts[1] };

        if (versionParts.Length != 3 && versionParts.Length == 1)
            versionParts = new[] { "1", "0", versionParts[0] };

        if (int.TryParse(versionParts[2], out var buildNumber))
        {
            if (buildNumber < 9999)
            {
                buildNumber++;
                versionParts[2] = buildNumber.ToString();
                return string.Join(".", versionParts);
            }
            else
            {
                if (int.TryParse(versionParts[1], out var minorVersionNumber))
                {
                    if (minorVersionNumber < 99)
                    {
                        minorVersionNumber++;
                        versionParts[1] = minorVersionNumber.ToString();
                        return string.Join(".", versionParts);
                    }
                    else
                    {
                        _ = int.TryParse(versionParts[1], out var majorVersionNumber);
                        majorVersionNumber++;
                        return $"{majorVersionNumber.ToString()}.0.0";
                    }
                }
            }
        }

        return versionValue;
    }
}