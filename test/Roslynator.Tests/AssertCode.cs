using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using static TestHelpers;

public static class AssertCode
{
    public static async Task NoErrorsAsync(Document document, CancellationToken cancellationToken = default(CancellationToken))
    {
        var compilation = await document.Project.GetCompilationAsync(cancellationToken);
        if (compilation.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning))
        {
            SyntaxNode syntax;
            try
            {
                // Attempt to normalize whitespace and get the errors again, so the code is more legible
                syntax = await document.GetSyntaxRootAsync(cancellationToken);
                syntax = syntax.NormalizeWhitespace();
                document = document.WithSyntaxRoot(syntax);
                compilation = await document.Project.GetCompilationAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                syntax = await document.GetSyntaxRootAsync(cancellationToken);
            }

            if (!string.IsNullOrEmpty(document.FilePath))
                File.WriteAllText(document.FilePath, (await document.GetTextAsync()).ToString());

            var indexOffset = document.Project.Language == LanguageNames.VisualBasic ? 1 : 0;

            Assert.False(true,
                Environment.NewLine +
                string.Join(
                    Environment.NewLine, 
                    compilation
                        .GetDiagnostics()
                        .Where(d => d.Severity == DiagnosticSeverity.Error || 
                                    d.Severity == DiagnosticSeverity.Warning)
                        .Select(d => $"'{syntax.GetText().GetSubText(d.Location.SourceSpan).ToString()}' : {d.ToString()}")) +
                Environment.NewLine +
                string.Join(Environment.NewLine,
                    syntax.ToString()
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Select((line, index) => $"{(index + indexOffset).ToString().PadLeft(3, ' ')}| {line}")));
        }
    }
}