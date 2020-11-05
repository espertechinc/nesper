///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.agg.@base;
using com.espertech.esper.common.@internal.epl.expression.agg.method;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.method.avg
{
	public class AggregationForgeFactoryAvg : AggregationForgeFactoryBase
	{
		private readonly ExprAvgNode parent;
	    private readonly Type childType;
	    private readonly DataInputOutputSerdeForge distinctSerde;
	    private readonly Type resultType;
	    private readonly MathContext optionalMathContext;
	    private AggregatorMethod aggregator;

	    public AggregationForgeFactoryAvg(ExprAvgNode parent, Type childType, DataInputOutputSerdeForge distinctSerde, MathContext optionalMathContext) {
	        this.parent = parent;
	        this.childType = childType;
	        this.distinctSerde = distinctSerde;
	        this.resultType = GetAvgAggregatorType(childType);
	        this.optionalMathContext = optionalMathContext;
	    }

	    public override Type ResultType => resultType;

	    public override ExprAggregateNodeBase AggregationExpression => parent;

	    public override MathContext OptionalMathContext => optionalMathContext;

	    public override AggregationPortableValidation AggregationPortableValidation => 
		    new AggregationPortableValidationAvg(
			    parent.IsDistinct, 
			    parent.HasFilter, 
			    parent.ChildNodes[0].Forge.EvaluationType);

	    public override void InitMethodForge(int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized, CodegenClassScope classScope) {
	        Type distinctValueType = !parent.IsDistinct ? null : childType;
	        if (resultType.IsBigInteger()) {
		        aggregator = new AggregatorAvgBig(
			        this,
			        col,
			        rowCtor,
			        membersColumnized,
			        classScope,
			        distinctValueType,
			        distinctSerde,
			        parent.HasFilter,
			        parent.OptionalFilter);
	        } else {
		        aggregator = new AggregatorAvgNumeric(
			        this,
			        col,
			        rowCtor,
			        membersColumnized,
			        classScope,
			        distinctValueType,
			        distinctSerde,
			        parent.HasFilter,
			        parent.OptionalFilter,
			        childType.GetBoxedType());
	        }
	    }

	    public override AggregatorMethod Aggregator => aggregator;

	    public override ExprForge[] GetMethodAggregationForge(bool join, EventType[] typesPerStream)
	    {
	        return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
	    }

	    private Type GetAvgAggregatorType(Type type)
	    {
		    if (type.IsDecimal()) {
			    return typeof(decimal?);
		    } else if (type.IsBigInteger()) {
			    return typeof(BigInteger?);
		    }
		    return typeof(double?);
	    }
	}
} // end of namespace
