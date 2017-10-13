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
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>
    /// 
    /// </summary>
    public interface ExprSubselectStrategy
    {
        ICollection<EventBean> EvaluateMatching(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext);
    }

    public class ProxyExprSubselectStrategy : ExprSubselectStrategy
    {
        public Func<EventBean[], ExprEvaluatorContext, ICollection<EventBean>> ProcEvaluateMatching;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyExprSubselectStrategy"/> class.
        /// </summary>
        public ProxyExprSubselectStrategy()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyExprSubselectStrategy"/> class.
        /// </summary>
        /// <param name="procEvaluateMatching">The evaluate matching.</param>
        public ProxyExprSubselectStrategy(Func<EventBean[], ExprEvaluatorContext, ICollection<EventBean>> procEvaluateMatching)
        {
            ProcEvaluateMatching = procEvaluateMatching;
        }

        /// <summary>
        /// Evaluates the matching.
        /// </summary>
        /// <param name="eventsPerStream">The events per stream.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <returns></returns>
        public ICollection<EventBean> EvaluateMatching(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            return ProcEvaluateMatching.Invoke(eventsPerStream, exprEvaluatorContext);
        }
    }
}
