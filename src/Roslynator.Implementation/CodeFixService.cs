using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Diagnostics
{
    [ExportWorkspaceService(typeof(ICodeFixService))]
    [Shared]
    class CodeFixService : ICodeFixService
    {
        IEnumerable<Lazy<CodeFixProvider, CodeChangeProviderMetadata>> codeFixProviders;

        [ImportingConstructor]
        public CodeFixService([ImportMany]IEnumerable<Lazy<CodeFixProvider, CodeChangeProviderMetadata>> codeFixProviders)
            => this.codeFixProviders = codeFixProviders;

        public async Task<ImmutableArray<ICodeFix>> GetCodeFixes(Document document, string codeFixerName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compilation = await document.Project.GetCompilationAsync(cancellationToken);
            var span = (await document.GetSyntaxRootAsync()).FullSpan;
            var codeFixProvider = codeFixProviders
                .Where(x => x.Metadata.Name == codeFixerName && x.Metadata.Languages.Contains(document.Project.Language))
                .Select(x => x.Value).FirstOrDefault();

            if (codeFixProvider == null)
                return ImmutableArray<ICodeFix>.Empty;

            var diagnostics = compilation.GetDiagnostics(cancellationToken).Where(x => codeFixProvider.FixableDiagnosticIds.Contains(x.Id));
            var codeFixes = new List<ICodeFix>();
            foreach (var diagnostic in diagnostics)
            {
                await codeFixProvider.RegisterCodeFixesAsync(
                    new CodeFixContext(document, diagnostic, 
                    (a, d) => codeFixes.Add(new CodeFixAdapter(a, d)), 
                    cancellationToken));
            }

            return codeFixes.ToImmutableArray();
        }
    }
}
