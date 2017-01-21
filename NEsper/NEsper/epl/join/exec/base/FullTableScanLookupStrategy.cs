///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.rep;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// Lookup on an unindexed table returning the full table as matching events.
    /// </summary>
    public class FullTableScanLookupStrategy : JoinExecTableLookupStrategy
    {
        private readonly UnindexedEventTable _eventIndex;
    
        /// <summary>Ctor. </summary>
        /// <param name="eventIndex">table to use</param>
        public FullTableScanLookupStrategy(UnindexedEventTable eventIndex)
        {
            _eventIndex = eventIndex;
        }
    
        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = new Mutable<ISet<EventBean>>();

            using (Instrument.With(
                i => i.QIndexJoinLookup(this, _eventIndex),
                i => i.AIndexJoinLookup(result.Value, null)))
            {
                result.Value = _eventIndex.EventSet;
                if (result.Value.IsEmpty())
                    result.Value = null;

                return result.Value;
            }
        }

        /// <summary>Returns the associated table. </summary>
        /// <value>table for lookup.</value>
        public UnindexedEventTable EventIndex
        {
            get { return _eventIndex; }
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return new LookupStrategyDesc(LookupStrategyType.FULLTABLESCAN, null); }
        }
    }
}
