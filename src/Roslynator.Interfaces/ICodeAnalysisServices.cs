using System.Collections.Generic;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Host
{
    /// <summary>
    /// Provides access to the registered <see cref="ILanguageService"/> and 
    /// <see cref="IWorkspaceService"/> in the host.
    /// </summary>
    public interface ICodeAnalysisServices : IWorkspaceService
    {
        /// <summary>
        /// Gets the language services that match the given parameters.
        /// </summary>
        /// <param name="language">Language the service registered for. See <see cref="LanguageNames"/>.</param>
        /// <param name="serviceType">The language service type, from its <see cref="ExportLanguageServiceAttribute.ServiceType"/> registration attribute.</param>
        /// <param name="layer">The service layer, from its <see cref="ExportLanguageServiceAttribute.Layer"/>. See also <see cref="ServiceLayer"/>.</param>
        IEnumerable<ILanguageService> GetLanguageServices(string language, string serviceType, string layer = ServiceLayer.Default);

        /// <summary>
        /// Gets the language services that match the given parameters.
        /// </summary>
        /// <typeparam name="TService">The language service type, which should match the <see cref="ExportLanguageServiceAttribute.ServiceType"/> registration attribute.</typeparam>
        /// <param name="language">Language the service registered for. See <see cref="LanguageNames"/>.</param>
        /// <param name="layer">The service layer, from its <see cref="ExportLanguageServiceAttribute.Layer"/>. See also <see cref="ServiceLayer"/>.</param>
        IEnumerable<TService> GetLanguageServices<TService>(string language, string layer = ServiceLayer.Default)
            where TService : ILanguageService;

        /// <summary>
        /// Gets the workspace service that matches the given parameters.
        /// </summary>
        /// <param name="serviceType">The workspace service type, from its <see cref="ExportWorkspaceServiceAttribute.ServiceType"/> registration attribute.</param>
        /// <param name="layer">The service layer, from its <see cref="ExportWorkspaceServiceAttribute.Layer"/>. See also <see cref="ServiceLayer"/>.</param>
        IWorkspaceService GetWorkspaceService(string serviceType, string layer = ServiceLayer.Default);

        /// <summary>
        /// Gets the workspace service of the given <typeparamref name="TService"/> for the specified laye (if any).
        /// </summary>
        /// <typeparam name="TService">The workspace service type, which should match the <see cref="ExportWorkspaceServiceAttribute.ServiceType"/> registration attribute.</typeparam>
        /// <param name="layer">The service layer, from its <see cref="ExportWorkspaceServiceAttribute.Layer"/>. See also <see cref="ServiceLayer"/>.</param>
        TService GetWorkspaceService<TService>(string layer = ServiceLayer.Default)
            where TService : class, IWorkspaceService;
    }
}