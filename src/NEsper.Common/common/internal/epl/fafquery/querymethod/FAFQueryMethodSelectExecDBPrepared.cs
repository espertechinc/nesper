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
    public class FAFQueryMethodSelectExecDBPrepared : FAFQueryMethodSelectExecDBBase
    {
        private FireAndForgetProcessorDBExecPrepared prepared;

        public FAFQueryMethodSelectExecDBPrepared(StatementContextRuntimeServices services) : base(services)
        {
        }

        public void Prepare(FAFQueryMethodSelect select)
        {
            var db = (FireAndForgetProcessorDB)select.Processors[0];
            ExprEvaluatorContext exprEvaluatorContext =
                new FAFQueryMethodSelectNoFromExprEvaluatorContext(services, select);
            prepared = db.Prepared(exprEvaluatorContext, services);
        }

        protected override ICollection<EventBean> ExecuteInternal(
            ExprEvaluatorContext exprEvaluatorContext,
            FAFQueryMethodSelect select)
        {
            return prepared.PerformQuery(exprEvaluatorContext);
        }

        public void Close()
        {
            prepared.Close();
        }
    }
} // end of namespace