//////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTClassIdentifierHelper
    {
        public static ClassDescriptor Walk(EsperEPL2GrammarParser.ClassIdentifierNoDimensionsContext ctx)
        {
            if (ctx == null) {
                return null;
            }

            var name = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
            if (ctx.typeParameters() == null) {
                return new ClassDescriptor(name);
            }

            var typeParameters = WalkTypeParameters(ctx.typeParameters());
            return new ClassDescriptor(name, typeParameters, 0, false);
        }

        public static ClassDescriptor Walk(EsperEPL2GrammarParser.ClassIdentifierWithDimensionsContext ctx)
        {
            if (ctx == null) {
                return null;
            }

            var name = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
            var dimensions = ctx.dimensions();

            if (dimensions.IsEmpty() && ctx.typeParameters() == null) {
                return new ClassDescriptor(name);
            }

            var typeParameters = WalkTypeParameters(ctx.typeParameters());
            if (dimensions.IsEmpty()) {
                return new ClassDescriptor(name, typeParameters, 0, false);
            }

            var first = dimensions[0];
            var keyword = first?.IDENT()?.ToString()?.Trim().ToLowerInvariant();
            if (keyword != null) {
                if (!keyword.Equals(ClassDescriptor.PRIMITIVE_KEYWORD)) {
                    throw ASTWalkException.From(
                        "Invalid array keyword '" +
                        keyword +
                        "', expected '" +
                        ClassDescriptor.PRIMITIVE_KEYWORD +
                        "'");
                }

                if (!typeParameters.IsEmpty()) {
                    throw ASTWalkException.From(
                        "Cannot use the '" + ClassDescriptor.PRIMITIVE_KEYWORD + "' keyword with type parameters");
                }
            }

            return new ClassDescriptor(name, typeParameters, dimensions.Length, keyword != null);
        }

        private static IList<ClassDescriptor> WalkTypeParameters(
            EsperEPL2GrammarParser.TypeParametersContext typeParameters)
        {
            if (typeParameters == null) {
                return EmptyList<ClassDescriptor>.Instance;
            }

            return typeParameters
                .classIdentifierWithDimensions()
                .Select(typeParamCtx => Walk(typeParamCtx))
                .ToList();
        }
    }
} // end of namespace