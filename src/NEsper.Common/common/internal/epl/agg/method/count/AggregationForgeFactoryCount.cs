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

namespace com.espertech.esper.common.@internal.epl.agg.method.count
{
	public class AggregationForgeFactoryCount : AggregationForgeFactoryBase
	{
		private readonly ExprCountNode _parent;
		private readonly bool _ignoreNulls;
		private readonly Type _countedValueType;
		private readonly DataInputOutputSerdeForge _distinctValueSerde;
		private AggregatorCount _aggregator;

		public AggregationForgeFactoryCount(
			ExprCountNode parent,
			bool ignoreNulls,
			Type countedValueType,
			DataInputOutputSerdeForge distinctValueSerde)
		{
			this._parent = parent;
			this._ignoreNulls = ignoreNulls;
			this._countedValueType = countedValueType;
			this._distinctValueSerde = distinctValueSerde;
		}

		public override Type ResultType => typeof(long?);

		public override void InitMethodForge(
			int col,
			CodegenCtor rowCtor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope)
		{
			Type distinctType = !_parent.IsDistinct ? null : _countedValueType;
			_aggregator = new AggregatorCount(
				this,
				col,
				rowCtor,
				membersColumnized,
				classScope,
				distinctType,
				_distinctValueSerde,
				_parent.HasFilter,
				_parent.OptionalFilter,
				false);
		}

		public override AggregatorMethod Aggregator => _aggregator;

		public override ExprForge[] GetMethodAggregationForge(
			bool join,
			EventType[] typesPerStream)
		{
			return GetMethodAggregationEvaluatorCountByForge(_parent.PositionalParams, join, typesPerStream);
		}

		private static ExprForge[] GetMethodAggregationEvaluatorCountByForge(
			ExprNode[] childNodes,
			bool join,
			EventType[] typesPerStream)
		{
			if (childNodes[0] is ExprWildcard && childNodes.Length == 2) {
				return ExprMethodAggUtil.GetDefaultForges(new ExprNode[] {childNodes[1]}, join, typesPerStream);
			}

			if (childNodes[0] is ExprWildcard && childNodes.Length == 1) {
				return ExprNodeUtilityQuery.EMPTY_FORGE_ARRAY;
			}

			return ExprMethodAggUtil.GetDefaultForges(childNodes, join, typesPerStream);
		}

		public override ExprAggregateNodeBase AggregationExpression => _parent;

		public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationCount(_parent.IsDistinct, false, _parent.IsDistinct, _countedValueType, _ignoreNulls);
	}
} // end of namespace
