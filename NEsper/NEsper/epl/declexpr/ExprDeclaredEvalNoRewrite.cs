///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.declexpr
{
    public class ExprDeclaredEvalNoRewrite
        : ExprDeclaredEvalBase
    {
        public ExprDeclaredEvalNoRewrite(ExprEvaluator innerEvaluator, ExpressionDeclItem prototype, bool isCache)
            : base(innerEvaluator, prototype, isCache)
        {
        }
    
        public override EventBean[] GetEventsPerStreamRewritten(EventBean[] eventsPerStream)
        {
            return eventsPerStream;
        }
    }
}
