using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;

namespace Microsoft.CodeAnalysis.CodeFixes
{
    /// <summary>
    /// An <see cref="IWorkspaceService"/> that can be retrieved from 
    /// <see cref="Workspace.Services"/> for retrieving and applying 
    /// code fixes.
    /// </summary>
    public interface ICodeFixService : IWorkspaceService
    {
        /// <summary>
        /// Retrieves the <see cref="CodeFixProvider"/>, if any, of the given 
        /// name for given language.
        /// </summary>
        /// <param name="language">The language supported by the code fix provider. See <see cref="LanguageNames"/>.</param>
        /// <param name="codeFixName">The name of the provider, such as the ones listed in <see cref="CodeFixNames"/>.</param>
        /// <returns>The provider, or <see langword="null"/>.</returns>
        CodeFixProvider GetCodeFixProvider(string language, string codeFixName);

        /// <summary>
        /// Gets all the available code fixes from all registered providers for the 
        /// specified document.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to retrieve code fixes for.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> for the operation.</param>
        /// <returns>Available code fixes, if any.</returns>
        Task<ImmutableArray<ICodeFix>> GetCodeFixes(Document document, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets the available code fixes from the given provider for the 
        /// specified document.
        /// </summary>
        /// <param name="document">The <see cref="Document"/> to retrieve code fixes for.</param>
        /// <param name="codeFixName">The name of a code fix provider, as listed in <see cref="CodeFixNames"/>.</param>
        /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> for the operation.</param>
        /// <returns>Available code fixes, if any.</returns>
        Task<ImmutableArray<ICodeFix>> GetCodeFixes(Document document, string codeFixName, CancellationToken cancellationToken = default(CancellationToken));
    }
}