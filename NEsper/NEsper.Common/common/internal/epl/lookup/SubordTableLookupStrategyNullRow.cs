///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    public class SubordTableLookupStrategyNullRow : SubordTableLookupStrategy
    {
        private static readonly ISet<EventBean> SINGLE_NULL_ROW_EVENT_SET = new HashSet<EventBean>();

        public static readonly SubordTableLookupStrategyNullRow INSTANCE = new SubordTableLookupStrategyNullRow();

        static SubordTableLookupStrategyNullRow()
        {
            SINGLE_NULL_ROW_EVENT_SET.Add(null);
        }

        private SubordTableLookupStrategyNullRow()
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

        public LookupStrategyDesc StrategyDesc => new LookupStrategyDesc(LookupStrategyType.NULLROWS);
    }
} // end of namespace