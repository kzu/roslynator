using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Diagnostics
{
    public static partial class Hosting
    {
        public static HostServices CreateHost(params Assembly[] additionalAssemblies)
        {
            var composition = new ContainerConfiguration()
                .WithAssemblies(MefHostServices
                    .DefaultAssemblies
                    .Add(typeof(Hosting).Assembly)
                    .Add(Assembly.LoadFrom("Roslyn.Services.Editor.UnitTests.dll"))
                    .AddRange(additionalAssemblies ?? Enumerable.Empty<Assembly>()))
                .CreateContainer();

            // Setup a mechanism to import the CompositionHost from anywhere if needed.
            composition.GetExport<CompositionContextExporter>().CompositionHost = composition;

            return MefHostServices.Create(composition);
        }
    }
}
