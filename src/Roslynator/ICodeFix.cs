using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeActions;

namespace Microsoft.CodeAnalysis.Diagnostics
{
    public interface ICodeFix
    {
        CodeAction Action { get; }

        ImmutableArray<Diagnostic> Diagnostics { get; }
    }
}
