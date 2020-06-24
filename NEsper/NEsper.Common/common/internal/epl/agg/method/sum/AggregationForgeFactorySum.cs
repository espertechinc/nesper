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

namespace com.espertech.esper.common.@internal.epl.agg.method.sum
{
	public class AggregationForgeFactorySum : AggregationForgeFactoryBase
	{
		private readonly ExprSumNode _parent;
		private readonly Type _resultType;
		private readonly Type _inputValueType;
		private readonly DataInputOutputSerdeForge _distinctSerde;
		private AggregatorMethod _aggregator;

		public AggregationForgeFactorySum(
			ExprSumNode parent,
			Type inputValueType,
			DataInputOutputSerdeForge distinctSerde)
		{
			_parent = parent;
			_inputValueType = inputValueType;
			_distinctSerde = distinctSerde;
			_resultType = GetSumAggregatorType(inputValueType);
		}

		public override Type ResultType => _resultType;

		public override ExprAggregateNodeBase AggregationExpression => _parent;

		public override void InitMethodForge(
			int col,
			CodegenCtor rowCtor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope)
		{
			Type distinctValueType = !_parent.IsDistinct ? null : _inputValueType;
			if (_resultType == typeof(BigInteger)) {
				_aggregator = new AggregatorSumBig(
					this,
					col,
					rowCtor,
					membersColumnized,
					classScope,
					distinctValueType,
					_distinctSerde,
					_parent.HasFilter,
					_parent.OptionalFilter,
					_resultType);
			}
			else {
				_aggregator = new AggregatorSumNonBig(
					this,
					col,
					rowCtor,
					membersColumnized,
					classScope,
					distinctValueType,
					_distinctSerde,
					_parent.HasFilter,
					_parent.OptionalFilter,
					_resultType);
			}
		}

		public override AggregatorMethod Aggregator => _aggregator;

		public override ExprForge[] GetMethodAggregationForge(
			bool join,
			EventType[] typesPerStream)
		{
			return ExprMethodAggUtil.GetDefaultForges(_parent.PositionalParams, join, typesPerStream);
		}

		public override AggregationPortableValidation AggregationPortableValidation => 
			new AggregationPortableValidationSum(_parent.IsDistinct, _parent.HasFilter, _inputValueType);

		private Type GetSumAggregatorType(Type type)
		{
			if (type.IsBigInteger()) {
				return typeof(BigInteger);
			}

			return GetMemberType(type).GetBoxedType();
		}

		internal static Coercer GetCoercerNonBigInt(Type inputValueType)
		{
			Coercer coercer;
			if (inputValueType.IsInt64()) {
				coercer = SimpleNumberCoercerFactory.CoercerLong.INSTANCE;
			}
			else if (inputValueType.IsInt32()) {
				coercer = SimpleNumberCoercerFactory.CoercerInt.INSTANCE;
			}
			else if (inputValueType.IsDecimal()) {
				coercer = SimpleNumberCoercerFactory.CoercerDecimal.INSTANCE;
			}
			else if (inputValueType.IsDouble()) {
				coercer = SimpleNumberCoercerFactory.CoercerDouble.INSTANCE;
			}
			else if (inputValueType.IsSingle()) {
				coercer = SimpleNumberCoercerFactory.CoercerFloat.INSTANCE;
			}
			else {
				coercer = SimpleNumberCoercerFactory.CoercerInt.INSTANCE;
			}

			return coercer;
		}

		internal static Type GetMemberType(Type inputValueType)
		{
			if (inputValueType.IsInt64()) {
				return typeof(long);
			}
			else if (inputValueType.IsInt32()) {
				return typeof(int);
			}
			else if (inputValueType.IsDecimal()) {
				return typeof(decimal);
			}
			else if (inputValueType.IsDouble()) {
				return typeof(double);
			}
			else if (inputValueType.IsSingle()) {
				return typeof(float);
			}
			else {
				return typeof(int);
			}
		}
	}
} // end of namespace
