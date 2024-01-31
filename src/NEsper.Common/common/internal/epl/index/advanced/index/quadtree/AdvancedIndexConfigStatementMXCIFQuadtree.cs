///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.quadtree
{
    public class AdvancedIndexConfigStatementMXCIFQuadtree : EventAdvancedIndexConfigStatement
    {
        public ExprEvaluator XEval { get; set; }

        public ExprEvaluator YEval { get; set; }

        public ExprEvaluator WidthEval { get; set; }

        public ExprEvaluator HeightEval { get; set; }
    }
} // end of namespace