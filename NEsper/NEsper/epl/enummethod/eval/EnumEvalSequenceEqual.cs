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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.enummethod.eval
{
    public class EnumEvalSequenceEqual
        : EnumEvalBase
        , EnumEval
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EnumEvalSequenceEqual(ExprEvaluator innerExpression, int streamCountIncoming)
            : base(innerExpression, streamCountIncoming)
        {
        }

        public object EvaluateEnumMethod(EventBean[] eventsLambda, ICollection<object> target, bool isNewData, ExprEvaluatorContext context)
        {
            var otherObj = InnerExpression.Evaluate(new EvaluateParams(eventsLambda, isNewData, context));
            if (otherObj == null)
            {
                return false;
            }

            var other = otherObj.Unwrap<object>(true);
            if (other == null)
            {
                Log.Warn("Enumeration method 'sequenceEqual' expected a Collection-type return value from its parameter but received '" + otherObj.GetType().GetCleanName() + "'");
                return false;
            }

            if (target.Count != other.Count)
            {
                return false;
            }

            if (target.IsEmpty())
            {
                return true;
            }

            var oneit = target.GetEnumerator();
            var twoit = other.GetEnumerator();
            for (int i = 0; i < target.Count; i++)
            {
                if (!oneit.MoveNext()) { throw new InvalidOperationException(); }
                if (!twoit.MoveNext()) { throw new InvalidOperationException(); }

                var one = oneit.Current;
                var two = twoit.Current;

                if (one == null)
                {
                    if (two != null)
                    {
                        return false;
                    }
                    continue;
                }
                if (two == null)
                {
                    return false;
                }

                if (!Equals(one, two))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
