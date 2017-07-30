using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Microsoft.CodeAnalysis.Diagnostics.Tests
{
    public class CodeFixServiceTests
    {
        ITestOutputHelper output;

        public CodeFixServiceTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public async Task WhenGettingCodeFix()
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
            var codeFixes = await codeFixService.GetCodeFixes(document, CodeFixNames.VisualBasic.AddOverloads, cancellationToken: TimeoutToken(5));

            Assert.True(codeFixes.Any(), $"Did not find expected 'AddOverloads' code fix.");
        }

        [Fact]
        public async Task WhenGettingAllCodeFixes()
        {
            var cs = @"
using System;

public abstract class BaseDisposable
{
   public abstract bool IsDisposed { get; }
}

public class Disposable : BaseDisposable, IDisposable
{
}
";

            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);

            var document = workspace.AddDocument(DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id),
                "code.cs",
                filePath: Path.GetTempFileName(),
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(cs), VersionStamp.Create()))));

            var service = workspace.Services.GetRequiredService<ICodeFixService>();
            var fixes = await service.GetCodeFixes(document, cancellationToken: TimeoutToken(5));

            var providedBy = fixes.GroupBy(x => x.Provider);

            Assert.True(providedBy.Any(x => x.Key == CodeFixNames.CSharp.ImplementInterface));
            Assert.True(providedBy.Any(x => x.Key == CodeFixNames.CSharp.ImplementAbstractClass));
        }

        [Fact]
        public void WhenGettingProvider()
        {
            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.CSharp);
            var service = workspace.Services.GetRequiredService<ICodeFixService>();

            var provider = service.GetCodeFixProvider(project.Language, CodeFixNames.All.ImplementInterface);

            Assert.NotNull(provider);
        }

        [Fact]
        public async Task WhenApplyingBasicCodeFix()
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
            var codeFixes = await codeFixService.GetCodeFixes(document, CodeFixNames.VisualBasic.AddOverloads, cancellationToken: TimeoutToken(5));

            foreach (var codeFix in codeFixes)
            {
                await codeFix.ApplyAsync(workspace, TimeoutToken(1));
            }

            document = workspace.CurrentSolution.GetDocument(document.Id);

            await AssertCode.NoErrorsAsync(document, TimeoutToken(2));
        }

        [Fact]
        public async Task WhenApplyingCSharpCodeFix()
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
            var codeFixes = await codeFixService.GetCodeFixes(document, CodeFixNames.All.ImplementInterface, cancellationToken: TimeoutToken(5));

            foreach (var codeFix in codeFixes)
            {
                await codeFix.ApplyAsync(workspace, TimeoutToken(1));
            }

            document = workspace.CurrentSolution.GetDocument(document.Id);

            await AssertCode.NoErrorsAsync(document, TimeoutToken(2));
        }
    }
}


