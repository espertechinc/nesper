///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.supportunit.epl
{
    public class SupportExprEvaluator : ExprEvaluator
    {
        public object Evaluate(EvaluateParams evaluateParams)
        {
            return evaluateParams.EventsPerStream[0].Get("BoolPrimitive");
        }

        public Type ReturnType
        {
            get { return typeof (bool?); }
        }
    }
}
