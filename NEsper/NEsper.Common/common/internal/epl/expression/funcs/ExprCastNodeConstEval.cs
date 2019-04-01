///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprCastNodeConstEval : ExprEvaluator
    {
        private readonly ExprCastNode parent;
        private readonly object theConstant;

        public ExprCastNodeConstEval(ExprCastNode parent, object theConstant)
        {
            this.parent = parent;
            this.theConstant = theConstant;
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return theConstant;
        }
    }
} // end of namespace