///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.database.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public abstract class FireAndForgetProcessorDBExecBase
    {
        private readonly ExprEvaluator _lookupValuesEval;

        public FireAndForgetProcessorDBExecBase(
            PollExecStrategyDBQuery poll,
            ExprEvaluator lookupValuesEval)
        {
            Poll = poll;
            _lookupValuesEval = lookupValuesEval;
        }

        public PollExecStrategyDBQuery Poll { get; protected set; }

        protected ICollection<EventBean> DoPoll(ExprEvaluatorContext exprEvaluatorContext)
        {
            exprEvaluatorContext.VariableManagementService.SetLocalVersion();
            object lookupValues = null;
            if (_lookupValuesEval != null) {
                lookupValues = _lookupValuesEval.Evaluate(
                    CollectionUtil.EVENTBEANARRAY_EMPTY,
                    true,
                    exprEvaluatorContext);
            }

            return Poll.Poll(lookupValues, exprEvaluatorContext);
        }
    }
} // end of namespace