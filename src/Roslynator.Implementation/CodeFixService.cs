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

        public CodeFixProvider GetCodeFixProvider(string language, string codeFixName)
            => codeFixProviders
                .Where(x => x.Metadata.Languages.Contains(language) && x.Metadata.Name == codeFixName)
                .Select(x => x.Value)
                .FirstOrDefault();

        public async Task<ImmutableArray<ICodeFix>> GetCodeFixes(Document document, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compilation = await document.Project.GetCompilationAsync(cancellationToken);
            var span = (await document.GetSyntaxRootAsync()).FullSpan;
            var diagnostics = new Lazy<ImmutableArray<Diagnostic>>(() => compilation.GetDiagnostics(cancellationToken));
            var providers = codeFixProviders
                .Where(x => x.Metadata.Languages.Contains(document.Project.Language));

            var codeFixes = new List<ICodeFix>();
            foreach (var provider in providers)
            {
                foreach (var diagnostic in diagnostics.Value.Where(x => provider.Value.FixableDiagnosticIds.Contains(x.Id)))
                {
                    await provider.Value.RegisterCodeFixesAsync(
                        new CodeFixContext(document, diagnostic,
                        (action, diag) => codeFixes.Add(new CodeFixAdapter(action, diag, provider.Metadata.Name)),
                        cancellationToken));
                }
            }

            return codeFixes.ToImmutableArray();
        }

        public async Task<ImmutableArray<ICodeFix>> GetCodeFixes(Document document, string codeFixName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compilation = await document.Project.GetCompilationAsync(cancellationToken);
            var span = (await document.GetSyntaxRootAsync()).FullSpan;
            var provider = GetCodeFixProvider(document.Project.Language, codeFixName);

            if (provider == null)
                return ImmutableArray<ICodeFix>.Empty;

            var diagnostics = compilation.GetDiagnostics(cancellationToken).Where(x => provider.FixableDiagnosticIds.Contains(x.Id));
            var codeFixes = new List<ICodeFix>();
            foreach (var diagnostic in diagnostics)
            {
                await provider.RegisterCodeFixesAsync(
                    new CodeFixContext(document, diagnostic, 
                    (action, diag) => codeFixes.Add(new CodeFixAdapter(action, diag, codeFixName)), 
                    cancellationToken));
            }

            return codeFixes.ToImmutableArray();
        }
    }
}

