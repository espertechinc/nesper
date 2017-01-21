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

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Implementation for a table lookup strategy that returns exactly one row but leaves that row as an undefined value.
    /// </summary>
    public class SubordTableLookupStrategyNullRow : SubordTableLookupStrategy
    {
        private static readonly ISet<EventBean> SingleNullRowEventSet = new HashSet<EventBean>();

        static SubordTableLookupStrategyNullRow()
        {
            SingleNullRowEventSet.Add(null);
        }

        public ICollection<EventBean> Lookup(EventBean[] events, ExprEvaluatorContext context)
        {
            return SingleNullRowEventSet;
        }

        public String ToQueryPlan()
        {
            return this.GetType().Name;
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return new LookupStrategyDesc(LookupStrategyType.NULLROWS, null); }
        }
    }
}
