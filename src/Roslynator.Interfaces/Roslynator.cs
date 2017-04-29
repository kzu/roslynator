using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

/// <summary>
/// Creates <see cref="HostServices"/> that contain the Roslynator 
/// extensions, accessible via the <see cref="Workspace.Services"/>.
/// </summary>
public static partial class Roslynator
{
    /// <summary>
    /// Creates the <see cref="HostServices"/> for using when creating 
    /// <see cref="Workspace"/>s and leveraging the services provided 
    /// by Roslynator, such as the <see cref="ICodeFixService"/> and 
    /// <see cref="ICompositionContextService"/>.
    /// </summary>
    /// <param name="additionalAssemblies">Optional additional assemblies 
    /// to inject into the host services via MEF.</param>
    public static HostServices CreateHost(params Assembly[] additionalAssemblies)
    {
        var composition = new ContainerConfiguration()
            .WithAssemblies(MefHostServices
                .DefaultAssemblies
                .Add(typeof(Roslynator).Assembly)
                .Add(Assembly.LoadFrom("Roslyn.Services.Editor.UnitTests.dll"))
                .AddRange(additionalAssemblies ?? Enumerable.Empty<Assembly>()))
            .CreateContainer();

        // Setup a mechanism to import the CompositionContext from anywhere if needed.
        composition.GetExport<CompositionContextExporter>().CompositionHost = composition;

        return MefHostServices.Create(composition);
    }
}
