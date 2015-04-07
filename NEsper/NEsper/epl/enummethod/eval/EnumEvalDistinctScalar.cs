///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalDistinctScalar
        : EnumEvalBase
        , EnumEval
    {
        public EnumEvalDistinctScalar(int streamCountIncoming)
            : base(streamCountIncoming)
        {
        }

        public Object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> target,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (target == null || target.Count < 2)
            {
                return target;
            }

            if (target is ISet<object>)
            {
                return target;
            }

            var set = new LinkedHashSet<Object>();
            foreach (Object entry in target)
            {
                set.Add(entry);
            }
            return set;
        }
    }
}