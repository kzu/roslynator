using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using static TestHelpers;

namespace Microsoft.CodeAnalysis.Diagnostics.Tests

{
    public class EndToEndTests
    {
        [Fact]
        public void CreatingHost()
        {
            var host = Hosting.CreateHost();

            Assert.NotNull(host);
        }

        [Fact]
        public void WhenCreatingWorkspaceThenCanRetrieveCompositionContext()
        {
            var (workspace, _) = CreateWorkspaceAndProject(LanguageNames.CSharp);

            var composition = workspace.Services.GetRequiredService<ICompositionContextService>().CompositionContext;

            Assert.NotNull(composition);
        }

        [Fact]
        public async Task WhenGettingCodeFixesThenFindsThem()
        {
            var vb = @"
Public Class TypeGetter
    Public Function [GetType](assembly As String, name As String)
        Return Nothing       
    End Function
End Class";

            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.VisualBasic);
            var document = workspace.AddDocument(DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id),
                "code.vb",
                filePath: Path.GetTempFileName(),
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(vb), VersionStamp.Create()))));

            var codeFixService = workspace.Services.GetRequiredService<ICodeFixService>();
            var codeFixes = await codeFixService.GetCodeFixes(document, CodeFixNames.VisualBasic.AddOverloads, TimeoutToken(5));

            Assert.True(codeFixes.Any(), $"Did not find expected 'AddOverloads' code fix.");
        }

        [Fact]
        public async Task WhenApplyingBasicCodeFixThenSucceeds()
        {
            var vb = @"
Public Class TypeGetter
    Public Function [GetType](assembly As String, name As String)
        Return Nothing       
    End Function
End Class";

            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.VisualBasic);

            var document = workspace.AddDocument(DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id),
                "code.vb", 
                filePath: Path.GetTempFileName(), 
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(vb), VersionStamp.Create()))));

            var codeFixService = workspace.Services.GetRequiredService<ICodeFixService>();
            var codeFixes = await codeFixService.GetCodeFixes(document, CodeFixNames.VisualBasic.AddOverloads, TimeoutToken(5));

            foreach (var codeFix in codeFixes)
            {
                await codeFix.ApplyAsync(workspace, TimeoutToken(1));
            }

            document = workspace.CurrentSolution.GetDocument(document.Id);

            await AssertCode.NoErrorsAsync(document, TimeoutToken(2));
        }

        [Fact]
        public async Task WhenApplyingCSharpCodeFixThenSucceeds()
        {
            var cs = @"
using System;

public class Disposable : IDisposable
{
}
";

            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);

            var document = workspace.AddDocument(DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id),
                "code.cs",
                filePath: Path.GetTempFileName(),
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(cs), VersionStamp.Create()))));

            var codeFixService = workspace.Services.GetRequiredService<ICodeFixService>();
            var codeFixes = await codeFixService.GetCodeFixes(document, CodeFixNames.All.ImplementInterface, TimeoutToken(5));

            foreach (var codeFix in codeFixes)
            {
                await codeFix.ApplyAsync(workspace, TimeoutToken(1));
            }

            document = workspace.CurrentSolution.GetDocument(document.Id);

            await AssertCode.NoErrorsAsync(document, TimeoutToken(2));
        }
    }
}


