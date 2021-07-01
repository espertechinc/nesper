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
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.@internal.epl.agg.method.minmax
{
	public class AggregationForgeFactoryMinMax : AggregationForgeFactoryBase
	{
		private readonly ExprMinMaxAggrNode parent;
		private readonly Type type;
		private readonly bool hasDataWindows;
		private readonly DataInputOutputSerdeForge serde;
		private readonly DataInputOutputSerdeForge distinctSerde;
		private AggregatorMethod aggregator;

		public AggregationForgeFactoryMinMax(
			ExprMinMaxAggrNode parent,
			Type type,
			bool hasDataWindows,
			DataInputOutputSerdeForge serde,
			DataInputOutputSerdeForge distinctSerde)
		{
			this.parent = parent;
			this.type = type;
			this.hasDataWindows = hasDataWindows;
			this.serde = serde;
			this.distinctSerde = distinctSerde;
		}

		public DataInputOutputSerdeForge Serde => serde;

		public override Type ResultType => type;

		public override ExprAggregateNodeBase AggregationExpression => parent;

		public override void InitMethodForge(
			int col,
			CodegenCtor rowCtor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope)
		{
			Type distinctType = !parent.IsDistinct ? null : type;
			if (!hasDataWindows) {
				aggregator = new AggregatorMinMaxEver(
					this,
					col,
					rowCtor,
					membersColumnized,
					classScope,
					distinctType,
					distinctSerde,
					parent.HasFilter,
					parent.OptionalFilter,
					serde);
			}
			else {
				aggregator = new AggregatorMinMax(
					this,
					col,
					rowCtor,
					membersColumnized,
					classScope,
					distinctType,
					distinctSerde,
					parent.HasFilter,
					parent.OptionalFilter);
			}
		}

		public override AggregatorMethod Aggregator => aggregator;

		public override AggregationPortableValidation AggregationPortableValidation =>
			new AggregationPortableValidationMinMax(
				parent.IsDistinct,
				parent.HasFilter,
				parent.ChildNodes[0].Forge.EvaluationType,
				parent.MinMaxTypeEnum,
				hasDataWindows);

		public override ExprForge[] GetMethodAggregationForge(
			bool join,
			EventType[] typesPerStream)
		{
			return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
		}

		public ExprMinMaxAggrNode Parent => parent;
	}
} // end of namespace
