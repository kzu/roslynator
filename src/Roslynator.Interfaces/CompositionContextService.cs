using System.Composition;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Diagnostics
{
    [ExportWorkspaceService(typeof(ICompositionContextService))]
    class CompositionContextService : ICompositionContextService
    {
        [ImportingConstructor]
        public CompositionContextService(CompositionContext composition)
            => CompositionContext = composition;

        public CompositionContext CompositionContext { get; set; }
    }
}
