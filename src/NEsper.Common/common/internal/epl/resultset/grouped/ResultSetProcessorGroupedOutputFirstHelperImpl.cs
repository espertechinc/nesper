///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.grouped
{
    public class ResultSetProcessorGroupedOutputFirstHelperImpl : ResultSetProcessorGroupedOutputFirstHelper
    {
        private readonly IDictionary<object, OutputConditionPolled> outputState =
            new HashMap<object, OutputConditionPolled>();

        public void Remove(object key)
        {
            outputState.Remove(key);
        }

        public OutputConditionPolled GetOrAllocate(
            object mk,
            ExprEvaluatorContext exprEvaluatorContext,
            OutputConditionPolledFactory factory)
        {
            var outputStateGroup = outputState.Get(mk);
            if (outputStateGroup == null) {
                outputStateGroup = factory.MakeNew(exprEvaluatorContext);
                outputState.Put(mk, outputStateGroup);
            }

            return outputStateGroup;
        }

        public OutputConditionPolled Get(object mk)
        {
            return outputState.Get(mk);
        }

        public void Put(
            object mk,
            OutputConditionPolled outputStateGroup)
        {
            outputState.Put(mk, outputStateGroup);
        }

        public void Destroy()
        {
            // no action required
        }
    }
} // end of namespace