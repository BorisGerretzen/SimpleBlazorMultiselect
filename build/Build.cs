using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Octokit.Internal;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Project = Nuke.Common.ProjectModel.Project;

[GitHubActions("test", GitHubActionsImage.UbuntuLatest, On = new[] { GitHubActionsTrigger.PullRequest, GitHubActionsTrigger.WorkflowDispatch }, InvokedTargets = new[] { nameof(Test) }, FetchDepth = 10000)]
[GitHubActions("publish", GitHubActionsImage.UbuntuLatest, On = new[] { GitHubActionsTrigger.WorkflowDispatch }, InvokedTargets = new[] { nameof(Pack), nameof(Push) }, ImportSecrets = new[] { nameof(NugetApiKey) }, FetchDepth = 10000)]
[GitHubActions("publish demo", 
    GitHubActionsImage.UbuntuLatest, 
    On = new[] { GitHubActionsTrigger.WorkflowDispatch, GitHubActionsTrigger.Push }, 
    InvokedTargets = new[] { nameof(Clean), nameof(BuildDemo), nameof(DeployDemo) }, 
    FetchDepth = 10000, 
    ImportSecrets = new[] {nameof(TokenGithub)},
    WritePermissions = new[] { GitHubActionsPermissions.Contents, GitHubActionsPermissions.Pages }
    
    )]
class Build : NukeBuild
{
    [Nuke.Common.Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitVersion] readonly GitVersion GitVersion;

    [Nuke.Common.Parameter("API Key for the NuGet server.")] [Secret] readonly string NugetApiKey;

    [Nuke.Common.Parameter("NuGet server URL.")] readonly string NugetSource = "https://api.nuget.org/v3/index.json";

    [Nuke.Common.Parameter("NuGet package version.")] readonly string PackageVersion;
    
    [Nuke.Common.Parameter("Github token.")] [Secret] readonly string TokenGithub;

    [GitRepository] readonly GitRepository Repository;

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath TempDirectory => RootDirectory / "temp";
    AbsolutePath DemoDirectory => TempDirectory / "demo";
    
    Project LibProject => Solution.GetProject("SimpleBlazorMultiselect");
    Project DemoProject => Solution.GetProject("SimpleBlazorMultiselect.Demo");
    
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            TempDirectory.CreateOrCleanDirectory();
            DemoDirectory.CreateOrCleanDirectory();
            SourceDirectory.GlobDirectories("**/bin", "**/obj")
                .ForEach(path =>
                {
                    Console.WriteLine($"Removing '{path}'");
                    path.DeleteDirectory();
                });
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
                .CombineWith(
                    from project in new[] { LibProject, DemoProject }
                    from framework in project.GetTargetFrameworks()
                    select new { project, framework }, (cs, v) => cs
                        .SetProjectFile(v.project.Path)
                        .SetFramework(v.framework)
                )
            );
        });

    Target BuildDemo => _ => _
        .DependsOn(Restore, Compile)
        .Before(DeployDemo)
        .Produces(ArtifactsDirectory / "demo.zip")
        .Executes(() =>
        {
            DotNetPublish(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetProject(DemoProject)
                .SetOutput(DemoDirectory)
            );

            var zipPath = ArtifactsDirectory / "demo.zip";
            zipPath.DeleteFile();
            (DemoDirectory / "wwwroot").CompressTo(zipPath);
        });

    Target DeployDemo => _ => _
        .Requires(() => Configuration == Configuration.Release)
        .Executes(async () =>
        {
            var credentials = new Credentials(TokenGithub);
            GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue(nameof(NukeBuild)),
                new InMemoryCredentialStore(credentials));
            var client = GitHubTasks.GitHubClient;

            var repoOwner = Repository.GetGitHubOwner();
            var repoName = Repository.GetGitHubName();

            var ghPagesBranch = await client.Repository.Branch.Get(repoOwner, repoName, "gh-pages");
            var latestCommit = await client.Git.Commit.Get(repoOwner, repoName, ghPagesBranch.Commit.Sha);
        
            var wwwroot = DemoDirectory / "wwwroot";
            var files = wwwroot.GlobFiles("**/*");
            var newTree = new NewTree();
            newTree.Tree.Add(new NewTreeItem
            {
                Path = ".nojekyll",
                Mode = "100644",
                Type = TreeType.Blob,
                Content = ""
            });
            
            newTree.Tree.Add(new NewTreeItem
            {
                Path = ".gitattributes",
                Mode = "100644",
                Type = TreeType.Blob,
                Content = "* binary"
            });
            var tasks = files.Select(async file =>
            {
                var bytes = file.ReadAllBytes();
                var base64String = Convert.ToBase64String(bytes);
                if (string.IsNullOrEmpty(base64String)) return;
                var blob = new NewBlob
                {
                    Content = base64String,
                    Encoding = EncodingType.Base64
                };

                var blobRef = await client.Git.Blob.Create(repoOwner, repoName, blob);
                newTree.Tree.Add(new NewTreeItem
                {
                    Path = file.ToString()!
                        .Replace(wwwroot.ToString()!, "")
                        .Replace("\\", "/")
                        .TrimStart('/'),
                    Mode = "100644",
                    Type = TreeType.Blob,
                    Sha = blobRef.Sha
                });
                Console.WriteLine($"Added '{file}' to commit");
            });
            
            await Task.WhenAll(tasks);
            
            Console.WriteLine("Creating new tree");
            var newTreeRef = await client.Git.Tree.Create(repoOwner, repoName, newTree);
            Console.WriteLine($"Created new tree '{newTreeRef.Sha}'");
            
            Console.WriteLine("Creating new commit");
            var newCommit = new NewCommit("Deploying to GitHub Pages", newTreeRef.Sha, latestCommit.Sha);
            var commitRef = await client.Git.Commit.Create(repoOwner, repoName, newCommit);
            Console.WriteLine($"Created new commit '{commitRef.Sha}'");
            
            Console.WriteLine("Updating reference");
            await client.Git.Reference.Update(repoOwner, repoName, "heads/gh-pages", new ReferenceUpdate(commitRef.Sha));
            Console.WriteLine("Reference updated");
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
                .SetProject(LibProject)
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