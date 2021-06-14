///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTClassIdentifierHelper
    {
        public static ClassIdentifierWArray Walk(EsperEPL2GrammarParser.ClassIdentifierWithDimensionsContext ctx)
        {
            if (ctx == null)
            {
                return null;
            }

            var name = ASTUtil.UnescapeClassIdent(ctx.classIdentifier());
            IList<EsperEPL2GrammarParser.DimensionsContext> dimensions = ctx.dimensions();

            if (dimensions.IsEmpty())
            {
                return new ClassIdentifierWArray(name);
            }

            var first = dimensions[0].IDENT();
            var keyword = first?.ToString().Trim().ToLowerInvariant();
            if (keyword != null && !keyword.Equals(ClassIdentifierWArray.PRIMITIVE_KEYWORD))
            {
                throw ASTWalkException.From("Invalid array keyword '" + keyword + "', expected '" + ClassIdentifierWArray.PRIMITIVE_KEYWORD + "'");
            }

            return new ClassIdentifierWArray(name, dimensions.Count, keyword != null);
        }
    }
} // end of namespace