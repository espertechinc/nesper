///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodSelectExecDBUnprepared : FAFQueryMethodSelectExecDBBase
    {
        public FAFQueryMethodSelectExecDBUnprepared(StatementContextRuntimeServices services) : base(services)
        {
        }

        protected override ICollection<EventBean> ExecuteInternal(
            ExprEvaluatorContext exprEvaluatorContext,
            FAFQueryMethodSelect select)
        {
            var db = (FireAndForgetProcessorDB)select.Processors[0];
            var unprepared = db.Unprepared(exprEvaluatorContext, services);
            return unprepared.PerformQuery(exprEvaluatorContext);
        }
    }
} // end of namespace