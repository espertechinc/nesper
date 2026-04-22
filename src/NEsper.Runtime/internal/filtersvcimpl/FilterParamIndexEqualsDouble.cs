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
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class FilterParamIndexEqualsDouble : FilterParamIndexEqualsTyped<double>
    {
        public FilterParamIndexEqualsDouble(
            ExprFilterSpecLookupable lookupable,
            IReaderWriterLock readWriteLock)
            : base(lookupable, readWriteLock)
        {
        }

        protected override double Unbox(object value) => (double) value;

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
            var attributeValue = Lookupable.Eval.Eval(theEvent, ctx);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterReverseIndex(this, attributeValue);
            }

            if (attributeValue == null) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }
                return;
            }

            EventEvaluator evaluator;
            using (ConstantsMapRwLock.ReadLock.AcquireScope()) {
                TypedMap.TryGetValue((double) attributeValue, out evaluator);
            }

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
}
