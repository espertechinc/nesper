///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Antlr4.Runtime;

using com.espertech.esper.compiler.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class ASTTypeExpressionAnnoHelper
    {
        public static string ExpectMayTypeAnno(EsperEPL2GrammarParser.TypeExpressionAnnotationContext ctx, CommonTokenStream tokenStream)
        {
            if (ctx == null)
            {
                return null;
            }
            string annoName = ctx.n.Text;
            if (!string.Equals(annoName, "type", StringComparison.InvariantCultureIgnoreCase))
            {
                throw ASTWalkException.From("Invalid annotation for property selection, expected 'type' but found '" + annoName + "'", tokenStream, ctx);
            }
            return ctx.v.Text;
        }
    }
} // end of namespace