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
        private readonly AggregatorMethod _aggregator;

        public AggregationForgeFactorySum(
            ExprSumNode parent,
            Type inputValueType,
            DataInputOutputSerdeForge distinctSerde)
        {
            _parent = parent;
            _inputValueType = inputValueType;
            _distinctSerde = distinctSerde;
            _resultType = GetSumAggregatorType(inputValueType);
            
            var distinctValueType = !parent.IsDistinct ? null : inputValueType;
            if (_resultType.IsTypeBigInteger()) {
                _aggregator = new AggregatorSumBigInteger(
                    distinctValueType,
                    distinctSerde,
                    parent.HasFilter,
                    parent.OptionalFilter,
                    _resultType);
            }
            else {
                _aggregator = new AggregatorSumNumeric(
                    distinctValueType,
                    distinctSerde,
                    parent.HasFilter,
                    parent.OptionalFilter,
                    _resultType);
            }
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(_parent.PositionalParams, join, typesPerStream);
        }

        private Type GetSumAggregatorType(Type type)
        {
            if (type.IsTypeBigInteger()) {
                return typeof(BigInteger);
            }

            return GetMemberType(type).GetBoxedType();
        }

        public static Coercer GetCoercerNonBigInt(Type inputValueType)
        {
            Coercer coercer;
			if (inputValueType.IsTypeInt64()) {
				coercer = SimpleNumberCoercerFactory.CoercerLong.INSTANCE;
            }
			else if (inputValueType.IsTypeInt32()) {
				coercer = SimpleNumberCoercerFactory.CoercerInt.INSTANCE;
            }
			else if (inputValueType.IsTypeDecimal()) {
				coercer = SimpleNumberCoercerFactory.CoercerDecimal.INSTANCE;
            }
			else if (inputValueType.IsTypeDouble()) {
				coercer = SimpleNumberCoercerFactory.CoercerDouble.INSTANCE;
			}
			else if (inputValueType.IsTypeSingle()) {
				coercer = SimpleNumberCoercerFactory.CoercerFloat.INSTANCE;
            }
            else {
				coercer = SimpleNumberCoercerFactory.CoercerInt.INSTANCE;
            }

            return coercer;
        }

        public static Type GetMemberType(Type inputValueType)
        {
            if (inputValueType.IsTypeInt64()) {
                return typeof(long);
            }
            else if (inputValueType.IsTypeInt32()) {
                return typeof(int);
            }
            else if (inputValueType.IsTypeDecimal()) {
                return typeof(decimal);
            }
            else if (inputValueType.IsTypeDouble()) {
                return typeof(double);
            }
            else if (inputValueType.IsTypeSingle()) {
                return typeof(float);
            }
            else {
                return typeof(int);
            }
        }

        public override Type ResultType => _resultType;

        public override ExprAggregateNodeBase AggregationExpression => _parent;

        public override AggregatorMethod Aggregator => _aggregator;

        public override AggregationPortableValidation AggregationPortableValidation =>
            new AggregationPortableValidationSum(_parent.IsDistinct, _parent.HasFilter, _inputValueType);
    }
} // end of namespace