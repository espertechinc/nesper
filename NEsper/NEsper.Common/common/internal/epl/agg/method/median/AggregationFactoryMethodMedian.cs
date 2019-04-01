///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.method.median
{
	public class AggregationFactoryMethodMedian : AggregationFactoryMethodBase {
	    protected internal readonly ExprMedianNode parent;
	    protected internal readonly Type aggregatedValueType;
	    private AggregatorMethod aggregator;

	    public AggregationFactoryMethodMedian(ExprMedianNode parent, Type aggregatedValueType) {
	        this.parent = parent;
	        this.aggregatedValueType = aggregatedValueType;
	    }

	    public override Type ResultType {
	        get => typeof(double?);
	    }

	    public override void InitMethodForge(int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized, CodegenClassScope classScope) {
	        Type distinctType = !parent.IsDistinct ? null : aggregatedValueType;
	        aggregator = new AggregatorMedian(this, col, rowCtor, membersColumnized, classScope, distinctType, parent.HasFilter, parent.OptionalFilter);
	    }

	    public override AggregatorMethod Aggregator {
	        get => aggregator;
	    }

	    public override ExprAggregateNodeBase AggregationExpression {
	        get => parent;
	    }

	    public override ExprForge[] GetMethodAggregationForge(bool join, EventType[] typesPerStream) {
	        return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
	    }

	    public override AggregationPortableValidation AggregationPortableValidation {
	        get => new AggregationPortableValidationMedian(parent.IsDistinct, parent.HasFilter, aggregatedValueType);
	    }
	}
} // end of namespace