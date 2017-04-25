using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeActions;

namespace Microsoft.CodeAnalysis.Diagnostics
{
    class CodeFixAdapter : ICodeFix
    {
        public CodeFixAdapter(CodeAction action, ImmutableArray<Diagnostic> diagnostics, string provider)
        {
            Action = action;
            Diagnostics = diagnostics;
            Provider = provider;
        }

        public CodeAction Action { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public string Provider { get; }
    }
}