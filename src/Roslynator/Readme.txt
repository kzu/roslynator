# Roslynator 9000

Exposes some Roslyn internals in a nice way, such as executing code fixes or retrieving the CompositionContext for a Workspace.

In particular, [applying code fixes outside of Visual Studio](https://github.com/dotnet/roslyn/issues/2020) isn't very well 
supported or intuitive right now in Roslyn.

Usage:

HostServices host = Roslynator.CreateHost();
var workspace = new AdHocWorkspace(host);

// Now you can access the MEF composition from the workspace, to do composition.GetExport<T> for example:
CompositionContext composition = workspace.Services.GetRequiredService<ICompositionContextService>().CompositionContext;

// Access and optionally apply all available code fixes of type "ImplementInterface" regardless 
// of the projects' language:

var codeFixService = workspace.Services.GetRequiredService<ICodeFixService>();
// CodeFixNames is a compile-time generated list of all known code fixes discovered in Roslyn.
var codeFixes = await codeFixService.GetCodeFixes(document, CodeFixNames.All.ImplementInterface);

foreach (var codeFix in codeFixes)
{
    await codeFix.ApplyAsync(workspace);
}