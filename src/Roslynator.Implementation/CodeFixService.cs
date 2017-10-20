using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.CodeFixes
{
    [ExportWorkspaceService(typeof(ICodeFixService))]
    [Shared]
    class CodeFixService : ICodeFixService
    {
        IEnumerable<Lazy<CodeFixProvider, CodeChangeProviderMetadata>> codeFixProviders;
        Lazy<ImmutableArray<DiagnosticAnalyzer>> builtInAnalyzers = new Lazy<ImmutableArray<DiagnosticAnalyzer>>(() =>
            MefHostServices
                .DefaultAssemblies
                .SelectMany(x => x.GetTypes().Where(t => typeof(DiagnosticAnalyzer).IsAssignableFrom(t)))
                .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                .Select(t => (DiagnosticAnalyzer)Activator.CreateInstance(t))
                // Add our own.
                .Concat(new [] { new OverridableMembersAnalyzer() })
                .ToImmutableArray());

        [ImportingConstructor]
        public CodeFixService([ImportMany]IEnumerable<Lazy<CodeFixProvider, CodeChangeProviderMetadata>> codeFixProviders)
            => this.codeFixProviders = codeFixProviders.Concat(new[] 
            {
                // We don't expose this provider, it's a built-in one that 
                // can be invoked explicitly. If we registered it, it would 
                // pollute the code editor all the time.
                new Lazy<CodeFixProvider, CodeChangeProviderMetadata>(
                    () => new OverrideAllMembersCodeFix(), 
                    new CodeChangeProviderMetadata(
                        nameof(OverrideAllMembersCodeFix),
                        null, null,
                        LanguageNames.CSharp, LanguageNames.VisualBasic))
            })
            .ToImmutableList();

        public CodeFixProvider GetCodeFixProvider(string language, string codeFixName)
            => codeFixProviders
                .Where(x => x.Metadata.Languages.Contains(language) && x.Metadata.Name == codeFixName)
                .Select(x => x.Value)
                .FirstOrDefault();

        public async Task<ImmutableArray<ICodeFix>> GetCodeFixes(Document document, ImmutableArray<DiagnosticAnalyzer> analyzers = default(ImmutableArray<DiagnosticAnalyzer>), CancellationToken cancellationToken = default(CancellationToken))
        {
            var compilation = await document.Project.GetCompilationAsync(cancellationToken);

            if (analyzers.IsDefaultOrEmpty)
                analyzers = builtInAnalyzers.Value;
            else
                analyzers = analyzers.Concat(builtInAnalyzers.Value);

            var analyerCompilation = compilation.WithAnalyzers(analyzers, cancellationToken: cancellationToken);

            var diagnostics = await analyerCompilation.GetAllDiagnosticsAsync(cancellationToken);
            var providers = codeFixProviders
                .Where(x => x.Metadata.Languages.Contains(document.Project.Language));

            var codeFixes = new List<ICodeFix>();
            foreach (var provider in providers)
            {
                foreach (var diagnostic in diagnostics
                    .Where(x => provider.Value.FixableDiagnosticIds.Contains(x.Id))
                    // Only consider the diagnostics raised by the target document.
                    .Where(d =>
                        d.Location.Kind == LocationKind.SourceFile &&
                        d.Location.GetLineSpan().Path == document.FilePath))
                {
                    await provider.Value.RegisterCodeFixesAsync(
                        new CodeFixContext(document, diagnostic,
                        (action, diag) => codeFixes.Add(new CodeFixAdapter(action, diag, provider.Metadata.Name)),
                        cancellationToken));
                }
            }

            return codeFixes.ToImmutableArray();
        }

        public async Task<ImmutableArray<ICodeFix>> GetCodeFixes(Document document, string codeFixName, ImmutableArray<DiagnosticAnalyzer> analyzers = default(ImmutableArray<DiagnosticAnalyzer>), CancellationToken cancellationToken = default(CancellationToken))
        {
            var provider = GetCodeFixProvider(document.Project.Language, codeFixName);
            if (provider == null)
                return ImmutableArray<ICodeFix>.Empty;

            var compilation = await document.Project.GetCompilationAsync(cancellationToken);

            if (analyzers.IsDefaultOrEmpty)
                analyzers = builtInAnalyzers.Value;
            else
                analyzers = analyzers.Concat(builtInAnalyzers.Value);

            var analyerCompilation = compilation.WithAnalyzers(analyzers, cancellationToken: cancellationToken);
            var allDiagnostics = await analyerCompilation.GetAllDiagnosticsAsync(cancellationToken);
            var diagnostics = allDiagnostics
                .Where(x => provider.FixableDiagnosticIds.Contains(x.Id))
                // Only consider the diagnostics raised by the target document.
                .Where(d =>
                    d.Location.Kind == LocationKind.SourceFile &&
                    d.Location.GetLineSpan().Path == document.FilePath);

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