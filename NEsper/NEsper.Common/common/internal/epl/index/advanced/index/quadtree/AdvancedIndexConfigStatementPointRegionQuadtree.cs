///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class AdvancedIndexConfigStatementPointRegionQuadtree : EventAdvancedIndexConfigStatement
    {
        public AdvancedIndexConfigStatementPointRegionQuadtree()
        {
        }

        public AdvancedIndexConfigStatementPointRegionQuadtree(
            ExprEvaluator xEval,
            ExprEvaluator yEval)
        {
            XEval = xEval;
            YEval = yEval;
        }

        public ExprEvaluator XEval { get; set; }

        public ExprEvaluator YEval { get; set; }
    }
} // end of namespace