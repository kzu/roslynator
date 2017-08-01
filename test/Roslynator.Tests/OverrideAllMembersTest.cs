using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using Xunit.Abstractions;
using static TestHelpers;

namespace Microsoft.CodeAnalysis.Diagnostics.Tests
{
    public class OverrideAllMembersTest
    {
        ITestOutputHelper output;

        public OverrideAllMembersTest(ITestOutputHelper output) => this.output = output;


        [Fact]
        public async Task WhenOverridingAllMembers_ThenOverridesVirtualObjectMembers()
        {
            var cs = @"
public class Foo
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

            document = await codeFixService.ApplyAsync("OverrideAllMembersCodeFix", document, TimeoutToken(5));

            //output.WriteLine(document.GetTextAsync().Result.ToString());

            var syntax = await document.GetSyntaxRootAsync(TimeoutToken(1));
            var members = syntax.DescendantNodes().Where(n => n.IsKind(CodeAnalysis.CSharp.SyntaxKind.MethodDeclaration)).Count();

            Assert.Equal(typeof(object).GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => m.IsVirtual).Count(), members);
        }

        [Fact]
        public async Task WhenOverridingAllMembers_ThenOverridesVirtualObjectMembers_VB()
        {
            var code = @"
Public Class Foo

End Class
";

            var (workspace, project) = CreateWorkspaceAndProject(LanguageNames.VisualBasic);
            var document = workspace.AddDocument(DocumentInfo.Create(
                DocumentId.CreateNewId(project.Id),
                "code.vb",
                filePath: Path.GetTempFileName(),
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code), VersionStamp.Create()))));

            var codeFixService = workspace.Services.GetRequiredService<ICodeFixService>();

            document = await codeFixService.ApplyAsync("OverrideAllMembersCodeFix", document, TimeoutToken(5));

            output.WriteLine(document.GetTextAsync().Result.ToString());

            var syntax = await document.GetSyntaxRootAsync(TimeoutToken(1));
            var members = syntax.DescendantNodes().Where(n => n.IsKind(CodeAnalysis.VisualBasic.SyntaxKind.FunctionBlock)).Count();

            Assert.Equal(typeof(object).GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m => m.IsVirtual).Count(), members);
        }
    }
}
