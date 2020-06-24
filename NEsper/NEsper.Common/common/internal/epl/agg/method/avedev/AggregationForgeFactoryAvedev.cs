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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.method.avedev
{
	public class AggregationForgeFactoryAvedev : AggregationForgeFactoryBase
	{
		private readonly ExprAvedevNode _parent;
		private readonly Type _aggregatedValueType;
		private readonly DataInputOutputSerdeForge _distinctSerde;
		private readonly ExprNode[] _positionalParameters;
		private AggregatorMethod _aggregator;

		public AggregationForgeFactoryAvedev(
			ExprAvedevNode parent,
			Type aggregatedValueType,
			DataInputOutputSerdeForge distinctSerde,
			ExprNode[] positionalParameters)
		{
			_parent = parent;
			_aggregatedValueType = aggregatedValueType;
			_distinctSerde = distinctSerde;
			_positionalParameters = positionalParameters;
		}

		public override Type ResultType => typeof(double?);

		public override void InitMethodForge(
			int col,
			CodegenCtor rowCtor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope)
		{
			Type distinctType = !_parent.IsDistinct ? null : _aggregatedValueType;
			_aggregator = new AggregatorAvedev(
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

		public override ExprAggregateNodeBase AggregationExpression => _parent;

		public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationAvedev(
			_parent.IsDistinct,
			_parent.HasFilter,
			_aggregatedValueType);

		public ExprNode[] PositionalParameters => _positionalParameters;

		public override ExprForge[] GetMethodAggregationForge(
			bool join,
			EventType[] typesPerStream)
		{
			return ExprMethodAggUtil.GetDefaultForges(_parent.PositionalParams, join, typesPerStream);
		}
	}
} // end of namespace
