using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;

namespace Microsoft.CodeAnalysis.CodeFixes
{
    /// <summary>
    /// Provides the <see cref="ApplyAsync"/> extension method for <see cref="ICodeFix"/> 
    /// to easily apply code fixes to a <see cref="Workspace"/>.
    /// </summary>
    public static class ICodeFixExtensions
    {
        /// <summary>
        /// Applies the <see cref="ICodeFix.Action"/> operations (retrieved from its 
        /// <see cref="CodeAction.GetOperationsAsync(CancellationToken)"/>) to the 
        /// given <see cref="Workspace"/>.
        /// </summary>
        /// <param name="codeFix">The code fix to apply to the <paramref name="workspace"/>.</param>
        /// <param name="workspace">The <see cref="Workspace"/> to apply the code fix to.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> for the operation.</param>
        public static async Task ApplyAsync(this ICodeFix codeFix, Workspace workspace, CancellationToken cancellationToken = default(CancellationToken))
        {
            var operations = await codeFix.Action.GetOperationsAsync(cancellationToken);
            var solution = workspace.CurrentSolution;
            foreach (var operation in operations)
            {
                if (operation is ApplyChangesOperation applyChanges)
                    applyChanges.Apply(workspace, cancellationToken);
            }
        }
    }
}
