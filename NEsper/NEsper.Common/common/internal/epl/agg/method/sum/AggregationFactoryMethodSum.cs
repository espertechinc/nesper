///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.method.sum
{
    public class AggregationFactoryMethodSum : AggregationFactoryMethodBase
    {
        internal readonly ExprSumNode parent;
        internal readonly Type resultType;
        internal readonly Type inputValueType;
        internal AggregatorMethod aggregator;

        public AggregationFactoryMethodSum(
            ExprSumNode parent,
            Type inputValueType)
        {
            this.parent = parent;
            this.inputValueType = inputValueType;
            this.resultType = GetSumAggregatorType(inputValueType);
        }

        public override Type ResultType {
            get => resultType;
        }

        public override ExprAggregateNodeBase AggregationExpression {
            get => parent;
        }

        public override void InitMethodForge(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            Type distinctValueType = !parent.IsDistinct ? null : inputValueType;
            if (resultType == typeof(BigInteger) || resultType == typeof(decimal)) {
                aggregator = new AggregatorSumBig(
                    this,
                    col,
                    rowCtor,
                    membersColumnized,
                    classScope,
                    distinctValueType,
                    parent.IsFilter,
                    parent.OptionalFilter,
                    resultType);
            }
            else {
                aggregator = new AggregatorSumNonBig(
                    this,
                    col,
                    rowCtor,
                    membersColumnized,
                    classScope,
                    distinctValueType,
                    parent.IsFilter,
                    parent.OptionalFilter,
                    resultType);
            }
        }

        public override AggregatorMethod Aggregator {
            get => aggregator;
        }

        public override ExprForge[] GetMethodAggregationForge(
            bool join,
            EventType[] typesPerStream)
        {
            return ExprMethodAggUtil.GetDefaultForges(parent.PositionalParams, join, typesPerStream);
        }

        public override AggregationPortableValidation AggregationPortableValidation {
            get => new AggregationPortableValidationSum(parent.IsDistinct, parent.IsFilter, inputValueType);
        }

        private Type GetSumAggregatorType(Type type)
        {
            if (type == typeof(BigInteger)) {
                return typeof(BigInteger);
            }

            if (type == typeof(decimal)) {
                return typeof(decimal);
            }

            return Boxing.GetBoxedType(GetMemberType(type));
        }

        protected internal static SimpleNumberCoercer GetCoercerNonBigIntDec(Type inputValueType)
        {
            SimpleNumberCoercer coercer;
            if (inputValueType == typeof(long?) || inputValueType == typeof(long)) {
                coercer = SimpleNumberCoercerFactory.CoercerLong.INSTANCE;
            }
            else if (inputValueType == typeof(int?) || inputValueType == typeof(int)) {
                coercer = SimpleNumberCoercerFactory.CoercerInt.INSTANCE;
            }
            else if (inputValueType == typeof(decimal?) || inputValueType == typeof(decimal)) {
                coercer = SimpleNumberCoercerFactory.CoercerDecimal.INSTANCE;
            }
            else if (inputValueType == typeof(double?) || inputValueType == typeof(double)) {
                coercer = SimpleNumberCoercerFactory.CoercerDouble.INSTANCE;
            }
            else if (inputValueType == typeof(float?) || inputValueType == typeof(float)) {
                coercer = SimpleNumberCoercerFactory.CoercerFloat.INSTANCE;
            }
            else {
                coercer = SimpleNumberCoercerFactory.CoercerInt.INSTANCE;
            }

            return coercer;
        }

        protected internal static Type GetMemberType(Type inputValueType)
        {
            if (inputValueType == typeof(long?) || inputValueType == typeof(long)) {
                return typeof(long);
            }
            else if (inputValueType == typeof(int?) || inputValueType == typeof(int)) {
                return typeof(int);
            }
            else if (inputValueType == typeof(decimal?) || inputValueType == typeof(decimal)) {
                return typeof(decimal);
            }
            else if (inputValueType == typeof(double?) || inputValueType == typeof(double)) {
                return typeof(double);
            }
            else if (inputValueType == typeof(float?) || inputValueType == typeof(float)) {
                return typeof(float);
            }
            else {
                return typeof(int);
            }
        }
    }
} // end of namespace