///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggfunc;
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

namespace com.espertech.esper.common.@internal.epl.agg.method.plugin
{
	public class AggregationForgeFactoryPlugin : AggregationForgeFactoryBase
	{
		private readonly ExprPlugInAggNode parent;
		private readonly AggregationFunctionForge aggregationFunctionForge;
		private readonly AggregationFunctionMode mode;
		private readonly Type aggregatedValueType;
		private readonly DataInputOutputSerdeForge distinctSerde;
		private AggregatorMethod aggregator;

		public AggregationForgeFactoryPlugin(
			ExprPlugInAggNode parent,
			AggregationFunctionForge aggregationFunctionForge,
			AggregationFunctionMode mode,
			Type aggregatedValueType,
			DataInputOutputSerdeForge distinctSerde)
		{
			this.parent = parent;
			this.aggregationFunctionForge = aggregationFunctionForge;
			this.mode = mode;
			this.aggregatedValueType = aggregatedValueType;
			this.distinctSerde = distinctSerde;
		}

		public override Type ResultType => aggregationFunctionForge.ValueType;

		public override ExprForge[] GetMethodAggregationForge(
			bool join,
			EventType[] typesPerStream)
		{
			return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
		}

		public override void InitMethodForge(
			int col,
			CodegenCtor rowCtor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope)
		{
			if (mode is AggregationFunctionModeManaged) {
				AggregationFunctionModeManaged singleValue = (AggregationFunctionModeManaged) mode;
				if (parent.PositionalParams.Length == 0) {
					throw new ArgumentException(typeof(AggregationFunctionModeManaged).Name + " requires at least one positional parameter");
				}

				Type distinctType = !parent.IsDistinct ? null : aggregatedValueType;
				aggregator = new AggregatorPlugInManaged(
					this,
					col,
					rowCtor,
					membersColumnized,
					classScope,
					distinctType,
					distinctSerde,
					parent.ChildNodes.Length > 1,
					parent.OptionalFilter,
					singleValue);
			}
			else if (mode is AggregationFunctionModeMultiParam) {
				AggregationFunctionModeMultiParam multiParam = (AggregationFunctionModeMultiParam) mode;
				aggregator = new AggregatorPlugInMultiParam(this, col, rowCtor, membersColumnized, classScope, multiParam);
			}
			else if (mode is AggregationFunctionModeCodeGenerated) {
				AggregationFunctionModeCodeGenerated codeGenerated = (AggregationFunctionModeCodeGenerated) mode;
				aggregator = codeGenerated.AggregatorMethodFactory.GetAggregatorMethod(
					new AggregatorMethodFactoryContext(col, rowCtor, membersColumnized, classScope));
			}
			else {
				throw new IllegalStateException("Received an unrecognized value for mode, the value is " + mode);
			}
		}

		public override AggregatorMethod Aggregator => aggregator;

		public override AggregationPortableValidation AggregationPortableValidation => new AggregationPortableValidationPlugin(parent.IsDistinct, parent.AggregationFunctionName);

		public override ExprAggregateNodeBase AggregationExpression => parent;

		public AggregationFunctionForge AggregationFunctionForge => aggregationFunctionForge;
	}
} // end of namespace
