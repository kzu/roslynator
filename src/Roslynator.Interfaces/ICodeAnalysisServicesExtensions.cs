using System.Linq;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Host
{
    /// <summary>
    /// Usability overloads for <see cref="ICodeAnalysisServices"/>.
    /// </summary>
    public static class ICodeAnalysisServicesExtensions
    {
        /// <summary>
        /// Gets the first (or none) language service that matches the given parameters. 
        /// See <see cref="ICodeAnalysisServices.GetLanguageServices(string, string, string)"/>.
        /// </summary>
        public static ILanguageService GetLanguageService(this ICodeAnalysisServices services, string language, string serviceType, string layer = ServiceLayer.Default)
            => services.GetLanguageServices(language, serviceType, layer).FirstOrDefault();

        /// <summary>
        /// Gets the first (or none) language service of the given type that matches the parameters. 
        /// See <see cref="ICodeAnalysisServices.GetLanguageServices{TService}(string, string)"/>.
        /// </summary>
        /// <typeparam name="TService">The language service type, which should match the <see cref="ExportLanguageServiceAttribute.ServiceType"/> registration attribute.</typeparam>
        public static TService GetLanguageService<TService>(this ICodeAnalysisServices services, string language, string layer = ServiceLayer.Default)
            where TService : ILanguageService
            => services.GetLanguageServices<TService>(language, layer).FirstOrDefault();
    }
}