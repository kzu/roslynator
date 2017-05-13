﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Host
{
    [Export(typeof(ICodeAnalysisServices))]
    [ExportWorkspaceService(typeof(ICodeAnalysisServices))]
    [Shared]
    class CodeAnalysisServices : IWorkspaceService, ICodeAnalysisServices
    {
        IEnumerable<Lazy<ILanguageService, LanguageServiceMetadata>> languageServices;
        IEnumerable<Lazy<IWorkspaceService, WorkspaceServiceMetadata>> workspaceServices;

        [ImportingConstructor]
        public CodeAnalysisServices(
            [ImportMany] IEnumerable<Lazy<ILanguageService, LanguageServiceMetadata>> languageServices,
            [ImportMany] IEnumerable<Lazy<IWorkspaceService, WorkspaceServiceMetadata>> workspaceServices)
        {
            this.languageServices = languageServices ?? throw new ArgumentNullException(nameof(languageServices));
            this.workspaceServices = workspaceServices ?? throw new ArgumentNullException(nameof(workspaceServices));
        }

        public IEnumerable<ILanguageService> GetLanguageServices(string language, string serviceType, string layer = ServiceLayer.Default)
            => languageServices
                .Where(x => 
                    x.Metadata.Language == language && 
                    x.Metadata.Layer == layer &&
                    x.Metadata.ServiceType.StartsWith(serviceType))
                .Select(x => x.Value);

        public IEnumerable<TService> GetLanguageServices<TService>(string language, string layer = ServiceLayer.Default)
            where TService : ILanguageService
            => languageServices
                .Where(x => 
                    x.Metadata.Language == language && 
                    x.Metadata.Layer == layer &&
                    x.Metadata.ServiceType.StartsWith(typeof(TService).FullName))
                .Select(x => x.Value)
                .OfType<TService>();

        public IWorkspaceService GetWorkspaceService(string serviceType, string layer = ServiceLayer.Default)
            => workspaceServices
                .Where(x =>
                    x.Metadata.Layer == layer &&
                    x.Metadata.ServiceType.StartsWith(serviceType))
                .Select(x => x.Value)
                .FirstOrDefault() ?? throw new ArgumentException($"Missing requested workspace service {serviceType} from layer {layer}.");

        public TService GetWorkspaceService<TService>(string layer = ServiceLayer.Default)
            where TService : class, IWorkspaceService
            => workspaceServices
                .Where(x =>
                    x.Metadata.Layer == layer &&
                    x.Metadata.ServiceType.StartsWith(typeof(TService).FullName))
                .Select(x => x.Value)
                .OfType<TService>()
                .FirstOrDefault() ?? throw new ArgumentException($"Missing requested workspace service {typeof(TService).FullName} from layer {layer}.");
    }
}
