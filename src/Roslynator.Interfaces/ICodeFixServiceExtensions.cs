using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;

namespace Microsoft.CodeAnalysis.CodeFixes
{
    /// <summary>
    /// Usability extension methods for <see cref="ICodeFixService"/>.
    /// </summary>
    public static class ICodeFixServiceExtensions
    {
        /// <summary>
        /// Applies the given named code fix to a document.
        /// </summary>
        public static async Task<Document> ApplyAsync(this ICodeFixService codeFixService, string codeFixName, Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            // If we request and process ALL codefixes at once, we'll get one for each 
            // diagnostics, which is one per non-implemented member of the interface/abstract 
            // base class, so we'd be applying unnecessary fixes after the first one.
            // So we re-retrieve them after each Apply, which will leave only the remaining 
            // ones.
            var codeFixes = await codeFixService.GetCodeFixes(document, codeFixName, cancellationToken: cancellationToken);
            while (codeFixes.Length != 0)
            {
                var operations = await codeFixes[0].Action.GetOperationsAsync(cancellationToken);
                ApplyChangesOperation operation;
                if ((operation = operations.OfType<ApplyChangesOperation>().FirstOrDefault()) != null)
                {
                    document = operation.ChangedSolution.GetDocument(document.Id);
                    // Retrieve the codefixes for the updated doc again.
                    codeFixes = await codeFixService.GetCodeFixes(document, codeFixName, cancellationToken: cancellationToken);
                }
                else
                {
                    // If we got no applicable code fixes, exit the loop and move on to the next codefix.
                    break;
                }
            }

            return document;
        }
    }
}