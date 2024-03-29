///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private readonly ExprCastNode _parent;
        private readonly object _theConstant;

        public ExprCastNodeConstEval(
            ExprCastNode parent,
            object theConstant)
        {
            _parent = parent;
            _theConstant = theConstant;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return _theConstant;
        }
    }
} // end of namespace