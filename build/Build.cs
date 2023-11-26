using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions("test", GitHubActionsImage.UbuntuLatest, On = new[] { GitHubActionsTrigger.PullRequest, GitHubActionsTrigger.WorkflowDispatch }, InvokedTargets = new[] { nameof(Test) }, FetchDepth = 10000)]
[GitHubActions("publish", GitHubActionsImage.UbuntuLatest, On = new[] { GitHubActionsTrigger.WorkflowDispatch }, InvokedTargets = new[] { nameof(Pack), nameof(Push) }, ImportSecrets = new[] { nameof(NugetApiKey) }, FetchDepth = 10000)]
class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitVersion] readonly GitVersion GitVersion;

    [Parameter("API Key for the NuGet server.")] [Secret] readonly string NugetApiKey;

    [Parameter("NuGet server URL.")] readonly string NugetSource = "https://api.nuget.org/v3/index.json";

    [Parameter("NuGet package version.")] readonly string PackageVersion;

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Project TargetProject => Solution.GetProject("SimpleBlazorMultiselect");

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(path => path.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(path => path.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution)
            );
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .EnableNoRestore()
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
            );

            DotNetPublish(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .CombineWith(
                    from project in new[] { TargetProject }
                    from framework in project.GetTargetFrameworks()
                    select new { project, framework }, (cs, v) => cs
                        .SetProject(v.project)
                        .SetFramework(v.framework)
                )
            );
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetFramework("net8.0")
                .EnableNoRestore()
                .EnableNoBuild()
            );
        });

    // ReSharper disable once UnusedMember.Local
    Target Pack => _ => _
        .DependsOn(Clean, Test)
        .Before(Push)
        .Requires(() => Configuration == Configuration.Release)
        .Executes(() =>
        {
            DotNetPack(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetProject(TargetProject)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetProperty("PackageVersion", PackageVersion ?? GitVersion.NuGetVersionV2)
            );
        });

    // ReSharper disable once UnusedMember.Local
    Target Push => _ => _
        .Executes(() =>
        {
            DotNetNuGetPush(s => s
                .SetSource(NugetSource)
                .SetApiKey(NugetApiKey)
                .CombineWith(ArtifactsDirectory.GlobFiles("*.nupkg"), (s, v) => s
                    .SetTargetPath(v)
                )
            );
        });

    public static int Main() => Execute<Build>(x => x.Test);
}