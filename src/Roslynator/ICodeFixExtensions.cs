using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;

namespace Microsoft.CodeAnalysis.Diagnostics
{
    public static class ICodeFixExtensions
    {
        public static async Task ApplyAsync(this ICodeFix codeFix, Workspace workspace, CancellationToken cancellationToken)
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
