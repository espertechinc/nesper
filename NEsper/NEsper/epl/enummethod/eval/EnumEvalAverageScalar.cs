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
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalAverageScalar
        : EnumEvalBase
        , EnumEval
    {
        public EnumEvalAverageScalar(int streamCountIncoming)
            : base(streamCountIncoming)
        {
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            double sum = 0d;
            int count = 0;
    
            foreach (Object next in target) {
                var num = next;
                if (num == null) {
                    continue;
                }
                count++;
                sum += num.AsDouble();
            }
    
            if (count == 0) {
                return null;
            }
            return sum / count;
        }
    }
}
