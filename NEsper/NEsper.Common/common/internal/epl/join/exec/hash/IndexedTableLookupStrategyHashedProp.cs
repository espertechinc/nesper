///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.indexlookupplan;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.hash
{
	public class IndexedTableLookupStrategyHashedProp : JoinExecTableLookupStrategy {
	    private readonly IndexedTableLookupPlanHashedOnlyFactory factory;
	    private readonly PropertyHashedEventTable index;

	    public IndexedTableLookupStrategyHashedProp(IndexedTableLookupPlanHashedOnlyFactory factory, PropertyHashedEventTable index) {
	        this.factory = factory;
	        this.index = index;
	    }

	    public PropertyHashedEventTable Index
	    {
	        get => index;
	    }

	    public ISet<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext) {
	        InstrumentationCommon instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
	        instrumentationCommon.QIndexJoinLookup(this, index);

	        object key = factory.EventPropertyValueGetter.Get(theEvent);
	        ISet<EventBean> events = index.Lookup(key);

	        instrumentationCommon.AIndexJoinLookup(events, key);
	        return events;
	    }

	    public override string ToString() {
	        return "IndexedTableLookupStrategySingleExpr evaluation" +
	                " index=(" + index + ')';
	    }

	    public LookupStrategyType LookupStrategyType
	    {
	        get => LookupStrategyType.MULTIPROP;
	    }
	}
} // end of namespace