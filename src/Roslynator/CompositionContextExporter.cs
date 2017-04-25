using System.Composition;
using System.Composition.Hosting;

namespace Microsoft.CodeAnalysis.Diagnostics
{
    public static partial class Hosting
    {
        [Export]
        [Shared]
        class CompositionContextExporter
        {
            [Export]
            public CompositionHost CompositionHost { get; set; }
        }
    }
}
