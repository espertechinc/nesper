///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public abstract class MethodConversionStrategyBase : MethodConversionStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected EventType eventType;

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        protected bool CheckNonNullArrayValue(
            object value,
            MethodTargetStrategy origin)
        {
            if (value == null) {
                Log.Warn(
                    "Expected non-null return result from " +
                    origin.Plan +
                    ", but received null array element value");
                return false;
            }

            return true;
        }

        public abstract IList<EventBean> Convert(
            object invocationResult,
            MethodTargetStrategy origin,
            ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace