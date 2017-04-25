using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeActions;

namespace Microsoft.CodeAnalysis.Diagnostics
{
    class CodeFixAdapter : ICodeFix
    {
        public CodeFixAdapter(CodeAction action, ImmutableArray<Diagnostic> diagnostics)
        {
            Action = action;
            Diagnostics = diagnostics;
        }

        public CodeAction Action { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }
    }
}