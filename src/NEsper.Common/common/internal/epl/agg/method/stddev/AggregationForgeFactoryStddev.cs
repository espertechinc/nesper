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

namespace com.espertech.esper.common.@internal.epl.agg.method.stddev
{
	public class AggregationForgeFactoryStddev : AggregationForgeFactoryBase
	{
		private readonly ExprStddevNode _parent;
		private readonly Type _aggregatedValueType;
		private readonly DataInputOutputSerdeForge _distinctSerde;
		private AggregatorMethod _aggregator;

		public AggregationForgeFactoryStddev(
			ExprStddevNode parent,
			Type aggregatedValueType,
			DataInputOutputSerdeForge distinctSerde)
		{
			_parent = parent;
			_aggregatedValueType = aggregatedValueType;
			_distinctSerde = distinctSerde;
		}

		public override Type ResultType => typeof(double?);

		public override ExprAggregateNodeBase AggregationExpression => _parent;

		public override void InitMethodForge(
			int col,
			CodegenCtor rowCtor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope)
		{
			Type distinctType = !_parent.IsDistinct ? null : _aggregatedValueType;
			_aggregator = new AggregatorStddev(
				this,
				col,
				rowCtor,
				membersColumnized,
				classScope,
				distinctType,
				_distinctSerde,
				_parent.HasFilter,
				_parent.OptionalFilter);
		}

		public override AggregatorMethod Aggregator => _aggregator;

		public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationStddev(_parent.IsDistinct, _parent.HasFilter, _aggregatedValueType);

		public override ExprForge[] GetMethodAggregationForge(
			bool join,
			EventType[] typesPerStream)
		{
			return ExprMethodAggUtil.GetDefaultForges(_parent.PositionalParams, join, typesPerStream);
		}
	}
} // end of namespace
