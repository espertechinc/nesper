///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;

namespace com.espertech.esper.common.@internal.epl.agg.method.rate
{
	public class AggregationForgeFactoryRate : AggregationForgeFactoryBase {
	    protected readonly ExprRateAggNode parent;
	    protected readonly bool isEver;
	    protected readonly long intervalTime;
	    protected readonly TimeAbacus timeAbacus;
	    protected AggregatorMethod aggregator;

	    public AggregationForgeFactoryRate(ExprRateAggNode parent, bool isEver, long intervalTime, TimeAbacus timeAbacus) {
	        this.parent = parent;
	        this.isEver = isEver;
	        this.intervalTime = intervalTime;
	        this.timeAbacus = timeAbacus;
	    }

	    public override Type ResultType => typeof(double?);

	    public override void InitMethodForge(int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized, CodegenClassScope classScope) {
	        if (isEver) {
	            aggregator = new AggregatorRateEver(this, col, rowCtor, membersColumnized, classScope, null, null, false, parent.OptionalFilter);
	        } else {
	            aggregator = new AggregatorRate(this, col, rowCtor, membersColumnized, classScope, null, null, false, parent.OptionalFilter);
	        }
	    }

	    public override AggregatorMethod Aggregator => aggregator;

	    public override ExprAggregateNodeBase AggregationExpression => parent;

	    public override ExprForge[] GetMethodAggregationForge(bool join, EventType[] typesPerStream) {
	        return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
	    }

	    public ExprRateAggNode Parent => parent;

	    public bool IsEver => isEver;

	    public long IntervalTime => intervalTime;

	    public TimeAbacus TimeAbacus => timeAbacus;

	    public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationRate(parent.IsDistinct, parent.OptionalFilter != null, typeof(int), intervalTime);
	}
} // end of namespace
