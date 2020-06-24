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

namespace com.espertech.esper.common.@internal.epl.agg.method.nth
{
	public class AggregationForgeFactoryNth : AggregationForgeFactoryBase
	{
		private readonly ExprNthAggNode parent;
		private readonly Type childType;
		private readonly DataInputOutputSerdeForge serde;
		private readonly DataInputOutputSerdeForge distinctSerde;
		private readonly int size;
		private AggregatorNth aggregator;

		public AggregationForgeFactoryNth(
			ExprNthAggNode parent,
			Type childType,
			DataInputOutputSerdeForge serde,
			DataInputOutputSerdeForge distinctSerde,
			int size)
		{
			this.parent = parent;
			this.childType = childType;
			this.serde = serde;
			this.distinctSerde = distinctSerde;
			this.size = size;
		}

		public DataInputOutputSerdeForge Serde => serde;

		public override Type ResultType => childType;

		public override void InitMethodForge(
			int col,
			CodegenCtor rowCtor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope)
		{
			Type distinctValueType = !parent.IsDistinct ? null : childType;
			aggregator = new AggregatorNth(this, col, rowCtor, membersColumnized, classScope, distinctValueType, distinctSerde, false, parent.OptionalFilter);
		}

		public override AggregatorMethod Aggregator => aggregator;

		public ExprNthAggNode Parent => parent;

		public override ExprAggregateNodeBase AggregationExpression => parent;

		public override AggregationPortableValidation AggregationPortableValidation =>
			new AggregationPortableValidationNth(parent.IsDistinct, parent.OptionalFilter != null, childType, size);

		public override ExprForge[] GetMethodAggregationForge(
			bool join,
			EventType[] typesPerStream)
		{
			return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
		}

		public Type ChildType => childType;

		public int SizeOfBuf => size + 1;
	}
} // end of namespace
