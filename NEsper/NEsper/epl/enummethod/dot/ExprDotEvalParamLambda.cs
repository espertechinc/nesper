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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.dot
{
    public class ExprDotEvalParamLambda : ExprDotEvalParam
    {
        public ExprDotEvalParamLambda(int parameterNum, ExprNode body, ExprEvaluator bodyEvaluator, int streamCountIncoming, IList<String> goesToNames, EventType[] goesToTypes)
            : base(parameterNum, body, bodyEvaluator)
        {
            StreamCountIncoming = streamCountIncoming;
            GoesToNames = goesToNames;
            GoesToTypes = goesToTypes;
        }

        public int StreamCountIncoming { get; private set; }

        public IList<string> GoesToNames { get; private set; }

        public EventType[] GoesToTypes { get; private set; }
    }
}
