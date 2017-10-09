///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Antlr4.Runtime;

using com.espertech.esper.epl.generated;

namespace com.espertech.esper.epl.parse
{
    public class ASTTypeExpressionAnnoHelper
    {
        public static string ExpectMayTypeAnno(
            EsperEPL2GrammarParser.TypeExpressionAnnotationContext ctx,
            CommonTokenStream tokenStream)
        {
            if (ctx == null)
            {
                return null;
            }
            string annoName = ctx.n.Text;
            if (!annoName.ToLowerInvariant().Equals("type"))
            {
                throw ASTWalkException.From(
                    "Invalid annotation for property selection, expected 'type' but found '" + annoName + "'",
                    tokenStream, ctx);
            }
            return ctx.v.Text;
        }
    }
} // end of namespace
