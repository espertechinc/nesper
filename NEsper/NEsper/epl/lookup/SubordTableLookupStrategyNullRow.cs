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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Implementation for a table lookup strategy that returns exactly one row
    /// but leaves that row as an undefined value.
    /// </summary>
    public class SubordTableLookupStrategyNullRow : SubordTableLookupStrategy
    {
        private static readonly ISet<EventBean> SINGLE_NULL_ROW_EVENT_SET = new HashSet<EventBean>();

        static SubordTableLookupStrategyNullRow()
        {
            SINGLE_NULL_ROW_EVENT_SET.Add(null);
        }

        public SubordTableLookupStrategyNullRow()
        {
        }

        public ICollection<EventBean> Lookup(EventBean[] events, ExprEvaluatorContext context)
        {
            return SINGLE_NULL_ROW_EVENT_SET;
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        public LookupStrategyDesc StrategyDesc => new LookupStrategyDesc(LookupStrategyType.NULLROWS, null);
    }
} // end of namespace
