using System;
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
    AbsolutePath SourceGeneratorOutPutDirectory => OutPutDirectory / "Sourcegenerator";
    AbsolutePath SourceGeneratorTestOutPutDirectory => OutPutDirectory / "SourcegeneratorUniTest";
    AbsolutePath PackageDirectory => RootDirectory / "PSPackages";
    AbsolutePath NugetPackageFiles => SourceGeneratorOutPutDirectory / $"{PackageName}*.*nupkg";

    Target Default => d => d.DependsOn(Finalize);


    Target Finalize => d => d
        .DependsOn(RunTest)
        .Executes(() =>
        {
            // cleanup the temporary directories
            Directory.Delete(SourceGeneratorOutPutDirectory, true);
            Directory.Delete(SourceGeneratorTestOutPutDirectory, true);
            Directory.Delete(PackageDirectory, true);
            
        });


    Target RunTest => d => d
        .DependsOn(BuildTestProject)
        .Executes(() =>
        {
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
            DotNetBuild(s => s
                            .SetProjectFile(Solution.GetProject(SourceGeneratorTestPath))
                            .SetConfiguration(Configuration)
                            .SetOutputDirectory(SourceGeneratorTestOutPutDirectory));
        });


    Target UpdatePackageReference => d => d
        .DependsOn(MovePackages)
        .Executes(() =>
        {
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

            DotNetRestore(s => s.SetProjectFile(Solution.GetProject(SourceGeneratorTestPath))
                              .SetProperty("RestorePackagesPath", PackageDirectory));
        });


    Target MovePackages => d => d
        .DependsOn(BuildSourcegenerator)
        .Executes(() =>
        {
            var files = SourceGeneratorOutPutDirectory.GlobFiles($"{PackageName}*.*nupkg");
            foreach (var file in files)
            {
                Log.Information($"Moving {file.Name} => {PackageDirectory}");
                file.CopyToDirectory(PackageDirectory, ExistsPolicy.FileOverwrite);
            }
        });


    Target BuildSourcegenerator => d => d
        .DependsOn(Clean)
        .Executes(() =>
        {
            // Restore and build first project, the source generator
            DotNetRestore(s => s.SetProjectFile(Solution.GetProject(SourceGeneratorPath))
                              .SetProperty("RestorePackagesPath", PackageDirectory));

            DotNetBuild(s => s
                            .SetProjectFile(Solution.GetProject(SourceGeneratorPath))
                            .SetConfiguration(Configuration)
                            .SetOutputDirectory(SourceGeneratorOutPutDirectory));
        });


    Target Clean =>
        x => x
            .DependsOn(Announce)
            .DependsOn(ModifyVersionNumber)
            .Executes(() =>
            {
                Log.Information("Clearing *.bin  and *.obj folders");
                SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());

                Log.Information($"Deleting {SourceGeneratorOutPutDirectory}");
                SourceGeneratorOutPutDirectory.DeleteDirectory();

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
                Console.WriteLine($"Configuration: {Configuration}");
                Console.WriteLine($"RootDirectory : {RootDirectory}");
                Console.WriteLine($"SourceDirectory : {SourceDirectory}");
                Console.WriteLine($"OutPutDirectory : {OutPutDirectory}");
                Console.WriteLine($"PublishDirectory : {PublishDirectory}");
                Console.WriteLine($"PackageDirectory : {PackageDirectory}");
                Console.WriteLine($"SourceGeneratorPath : {SourceGeneratorPath}");
                Console.WriteLine($"SourceGeneratorTestPath : {SourceGeneratorTestPath}");
                Console.WriteLine($"NugetPackageFiles : {NugetPackageFiles}");
            });


    Target ModifyVersionNumber =>
        d => d
            .Executes(() =>
            {
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