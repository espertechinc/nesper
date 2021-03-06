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

namespace com.espertech.esper.common.@internal.epl.agg.method.leaving
{
	public class AggregationForgeFactoryLeaving : AggregationForgeFactoryBase
	{
		private readonly ExprLeavingAggNode parent;
		private AggregatorLeaving forge;

		public AggregationForgeFactoryLeaving(ExprLeavingAggNode parent)
		{
			this.parent = parent;
		}

		public override Type ResultType => typeof(bool?);

		public override ExprAggregateNodeBase AggregationExpression => parent;

		public override void InitMethodForge(
			int col,
			CodegenCtor rowCtor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope)
		{
			forge = new AggregatorLeaving(this, col, membersColumnized);
		}

		public override AggregatorMethod Aggregator => forge;

		public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationLeaving(
			parent.IsDistinct,
			parent.OptionalFilter != null,
			typeof(bool));

		public override ExprForge[] GetMethodAggregationForge(
			bool join,
			EventType[] typesPerStream)
		{
			return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
		}
	}
} // end of namespace
