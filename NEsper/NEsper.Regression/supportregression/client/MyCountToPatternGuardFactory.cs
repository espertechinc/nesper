///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.guard;
using com.espertech.esper.util;

namespace com.espertech.esper.supportregression.client
{
    public class MyCountToPatternGuardFactory : GuardFactorySupport
    {
        private ExprNode _numCountToExpr;
        private MatchedEventConvertor _convertor;
    
        public override void SetGuardParameters(IList<ExprNode> guardParameters, MatchedEventConvertor convertor)
        {
            const string message = "Count-to guard takes a single integer-value expression as parameter";
            if (guardParameters.Count != 1)
            {
                throw new GuardParameterException(message);
            }
    
            if (guardParameters[0].ExprEvaluator.ReturnType.IsNotInt32())
            {
                throw new GuardParameterException(message);
            }

            _numCountToExpr = guardParameters[0];
            _convertor = convertor;
        }
    
        public override Guard MakeGuard(PatternAgentInstanceContext context, MatchedEventMap beginState, Quitable quitable, EvalStateNodeNumber stateNodeId, Object guardState) {
            Object parameter = PatternExpressionUtil.Evaluate("Count-to guard", beginState, _numCountToExpr, _convertor, null);
            if (parameter == null)
            {
                throw new EPException("Count-to guard parameter evaluated to a null value");
            }

            int numCountTo = parameter.AsInt();
            return new MyCountToPatternGuard(numCountTo, quitable);
        }
    }
}
