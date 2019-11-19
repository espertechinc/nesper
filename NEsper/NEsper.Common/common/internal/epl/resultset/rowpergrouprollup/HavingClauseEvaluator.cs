///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    public interface HavingClauseEvaluator
    {
        bool EvaluateHaving(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);
    }

    public class ProxyHavingClauseEvaluator : HavingClauseEvaluator
    {
        public delegate bool EvaluateHavingFunc(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        public EvaluateHavingFunc ProcEvaluateHaving { get; set; }

        public ProxyHavingClauseEvaluator()
        {
        }

        public ProxyHavingClauseEvaluator(EvaluateHavingFunc procEvaluateHaving)
        {
            ProcEvaluateHaving = procEvaluateHaving;
        }

        public bool EvaluateHaving(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return ProcEvaluateHaving.Invoke(eventsPerStream, isNewData, exprEvaluatorContext);
        }
    }
} // end of namespace