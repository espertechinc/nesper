///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.epl.resultset.grouped
{
    public interface ResultSetProcessorGroupedOutputFirstHelper : ResultSetProcessorOutputHelper
    {
        OutputConditionPolled GetOrAllocate(
            object mk,
            ExprEvaluatorContext exprEvaluatorContext,
            OutputConditionPolledFactory optionalOutputFirstConditionFactory);

        void Remove(object key);

        void Destroy();
    }
} // end of namespace