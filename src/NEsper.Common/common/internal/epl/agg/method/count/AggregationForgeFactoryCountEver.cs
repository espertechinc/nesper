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
	public class AggregationForgeFactoryCountEver : AggregationForgeFactoryBase
	{
		private readonly ExprCountEverNode _parent;
		private readonly bool _ignoreNulls;
		private readonly Type _childType;
		private readonly DataInputOutputSerdeForge _distinctSerde;
		private AggregatorCount _aggregator;

		public AggregationForgeFactoryCountEver(
			ExprCountEverNode parent,
			bool ignoreNulls,
			Type childType,
			DataInputOutputSerdeForge distinctSerde)
		{
			this._parent = parent;
			this._ignoreNulls = ignoreNulls;
			this._childType = childType;
			this._distinctSerde = distinctSerde;
		}

		public override Type ResultType => typeof(long);

		public override ExprAggregateNodeBase AggregationExpression => _parent;

		public override void InitMethodForge(
			int col,
			CodegenCtor rowCtor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope)
		{
			Type distinctType = !_parent.IsDistinct ? null : _childType;
			_aggregator = new AggregatorCount(
				this,
				col,
				rowCtor,
				membersColumnized,
				classScope,
				distinctType,
				_distinctSerde,
				_parent.OptionalFilter != null,
				_parent.OptionalFilter,
				true);
		}

		public override AggregatorMethod Aggregator => _aggregator;

		public override AggregationPortableValidation AggregationPortableValidation {
			get {
				Type distinctType = !_parent.IsDistinct ? null : _parent.ChildNodes[0].Forge.EvaluationType;
				return new AggregationPortableValidationCount(_parent.IsDistinct, _parent.OptionalFilter != null, true, distinctType, _ignoreNulls);
			}
		}

		public override ExprForge[] GetMethodAggregationForge(
			bool join,
			EventType[] typesPerStream)
		{
			return ExprMethodAggUtil.GetDefaultForges(_parent.PositionalParams, join, typesPerStream);
		}
	}
} // end of namespace
