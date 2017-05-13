using System.Composition;

namespace Microsoft.CodeAnalysis.Host
{
    /// <summary>
    /// Allows retrieving MEF services directly from a <see cref="Workspace.Services"/> 
    /// by exposing the <see cref="System.Composition.CompositionContext"/> directly.
    /// </summary>
    public interface ICompositionContextService : IWorkspaceService
    {
        /// <summary>
        /// The <see cref="System.Composition.CompositionContext"/> used to 
        /// compose MEF components in use in the current <see cref="Workspace"/>.
        /// </summary>
        CompositionContext CompositionContext { get; }
    }
}