using System.Composition;
using Microsoft.CodeAnalysis.Host;

namespace Microsoft.CodeAnalysis.Diagnostics
{
    public interface ICompositionContextService : IWorkspaceService
    {
        CompositionContext CompositionContext { get; }
    }
}