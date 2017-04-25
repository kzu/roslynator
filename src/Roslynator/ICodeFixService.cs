using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;

namespace Microsoft.CodeAnalysis.Diagnostics
{
    public interface ICodeFixService : IWorkspaceService
    {
        Task<ImmutableArray<ICodeFix>> GetCodeFixes(Document document, string codeFixName, CancellationToken cancellationToken = default(CancellationToken));
    }
}