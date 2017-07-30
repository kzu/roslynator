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
    public class CompositionTests
    {
        ITestOutputHelper output;

        public CompositionTests(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void CreatingHost()
        {
            var host = Roslynator.CreateHost();

            Assert.NotNull(host);
        }

        [Fact]
        public void WhenGettingCompositionContext()
        {
            var (workspace, _) = CreateWorkspaceAndProject(LanguageNames.CSharp);

            var composition = workspace.Services.GetRequiredService<ICompositionContextService>().CompositionContext;

            Assert.NotNull(composition);
        }
    }
}