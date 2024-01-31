///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Index for filter parameter constants to match using the equals (=) operator.
    ///     The implementation is based on a regular HashMap.
    /// </summary>
    public class FilterParamIndexEquals : FilterParamIndexEqualsBase
    {
        public FilterParamIndexEquals(
            ExprFilterSpecLookupable lookupable,
            IReaderWriterLock readWriteLock)
            : base(lookupable, readWriteLock, FilterOperator.EQUAL)
        {
        }

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
            var attributeValue = Lookupable.Eval.Eval(theEvent, ctx);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterReverseIndex(this, attributeValue);
            }

            if (attributeValue == null) { //  null cannot match, not even null: requires use of "is"
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            // Look up in hashtable
            EventEvaluator evaluator = null;
            using (ConstantsMapRwLock.ReadLock.Acquire()) {
                evaluator = ConstantsMap.Get(attributeValue);
            }

            // No listener found for the value, return
            if (evaluator == null) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AFilterReverseIndex(true);
            }

            evaluator.MatchEvent(theEvent, matches, ctx);
        }
    }
} // end of namespace