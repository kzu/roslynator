using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;

static class TestHelpers
{
    public static (AdhocWorkspace workspace, Project project) CreateWorkspaceAndProject(string language)
    {
        var host = Hosting.CreateHost(typeof(TestHelpers).Assembly);
        var workspace = new AdhocWorkspace(host, WorkspaceKind.Host);
        var options = language == LanguageNames.CSharp ?
                (CompilationOptions)new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary) :
                (CompilationOptions)new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        var project = workspace.AddProject(ProjectInfo.Create(
            ProjectId.CreateNewId(), 
            VersionStamp.Create(),
            "code", 
            "code.dll", 
            language, 
            compilationOptions: options,
            metadataReferences: ReferencePaths.Paths
                .Select(path => MetadataReference.CreateFromFile(path))));

        return (workspace, project);
    }

    public static CancellationToken TimeoutToken(int seconds)
        => Debugger.IsAttached ?
            new CancellationTokenSource().Token :
            new CancellationTokenSource(TimeSpan.FromSeconds(seconds)).Token;
}