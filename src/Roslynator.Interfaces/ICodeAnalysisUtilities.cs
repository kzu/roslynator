using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis
{
    /// <summary>
    /// Provides miscelaneous utilities.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ICodeAnalysisUtilities
    {
        /// <summary>
        /// Gets the set of members in the inheritance chain of <paramref name="containingType"/> that
        /// are overridable.  The members will be returned in furthest-base type to closest-base
        /// type order.  i.e. the overridable members of <see cref="System.Object"/> will be at the start
        /// of the list, and the members of the direct parent type of <paramref name="containingType"/> 
        /// will be at the end of the list.
        /// 
        /// If a member has already been overridden (in <paramref name="containingType"/> or any base type) 
        /// it will not be included in the list.
        /// </summary>
        ImmutableArray<ISymbol> GetOverridableMembers(INamedTypeSymbol containingType, CancellationToken cancellationToken);
    }
}
