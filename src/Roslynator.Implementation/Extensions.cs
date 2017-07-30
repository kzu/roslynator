using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis
{
    // See: http://source.roslyn.io/#q=GetOverridableMembers
    static class Extensions
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
        public static ImmutableArray<ISymbol> GetOverridableMembers(
            this INamedTypeSymbol containingType, CancellationToken cancellationToken)
        {
            // Keep track of the symbols we've seen and what order we saw them in.  The 
            // order allows us to produce the symbols in the end from the furthest base-type
            // to the closest base-type
            var result = new Dictionary<ISymbol, int>();
            var index = 0;

            if (containingType != null && !containingType.IsScriptClass && !containingType.IsImplicitClass)
            {
                if (containingType.TypeKind == TypeKind.Class || containingType.TypeKind == TypeKind.Struct)
                {
                    var baseTypes = containingType.GetBaseTypes().Reverse();
                    foreach (var type in baseTypes)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Prefer overrides in derived classes
                        RemoveOverriddenMembers(result, type, cancellationToken);

                        // Retain overridable methods
                        AddOverridableMembers(result, containingType, type, ref index, cancellationToken);
                    }

                    // Don't suggest already overridden members
                    RemoveOverriddenMembers(result, containingType, cancellationToken);
                }
            }

            return result.Keys.OrderBy(s => result[s]).ToImmutableArray();
        }

        private static void AddOverridableMembers(
            Dictionary<ISymbol, int> result, INamedTypeSymbol containingType,
            INamedTypeSymbol type, ref int index, CancellationToken cancellationToken)
        {
            foreach (var member in type.GetMembers())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (IsOverridable(member, containingType))
                {
                    result[member] = index++;
                }
            }
        }

        private static bool IsOverridable(ISymbol member, INamedTypeSymbol containingType)
        {
            if (member.IsAbstract || member.IsVirtual || member.IsOverride)
            {
                if (member.IsSealed)
                {
                    return false;
                }

                if (!member.IsAccessibleWithin(containingType))
                {
                    return false;
                }

                switch (member.Kind)
                {
                    case SymbolKind.Event:
                        return true;
                    case SymbolKind.Method:
                        return ((IMethodSymbol)member).MethodKind == MethodKind.Ordinary;
                    case SymbolKind.Property:
                        return !((IPropertySymbol)member).IsWithEvents;
                }
            }

            return false;
        }

        private static void RemoveOverriddenMembers(
            Dictionary<ISymbol, int> result, INamedTypeSymbol containingType, CancellationToken cancellationToken)
        {
            foreach (var member in containingType.GetMembers())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var overriddenMember = member.OverriddenMember();
                if (overriddenMember != null)
                {
                    result.Remove(overriddenMember);
                }
            }
        }
    }
}
