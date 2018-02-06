///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.index.quadtree
{
    public class AdvancedIndexConfigStatementPointRegionQuadtree : EventAdvancedIndexConfigStatement
    {
        public AdvancedIndexConfigStatementPointRegionQuadtree(ExprEvaluator xEval, ExprEvaluator yEval)
        {
            XEval = xEval;
            YEval = yEval;
        }

        public ExprEvaluator XEval { get; }

        public ExprEvaluator YEval { get; }
    }
} // end of namespace