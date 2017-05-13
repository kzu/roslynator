using System.Composition;
using System.Composition.Hosting;

namespace Microsoft.CodeAnalysis.Host
{
    /// <summary>
    /// Internal component that exports via regular MEF the 
    /// <see cref="System.Composition.CompositionContext"/> for consumption 
    /// in the <see cref="ICompositionContextService"/> as 
    /// an <see cref="IWorkspaceService"/>.
    /// </summary>
    [Export]
    [Shared]
    class CompositionContextExporter
    {
        [Export]
        public CompositionHost CompositionHost { get; set; }
    }
}
