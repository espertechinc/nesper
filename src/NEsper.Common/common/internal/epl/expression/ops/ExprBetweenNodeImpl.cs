///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Represents the between-clause function in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprBetweenNodeImpl : ExprNodeBase,
        ExprBetweenNode
    {
        [NonSerialized] private ExprBetweenNodeForge _forge;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="lowEndpointIncluded">
        ///     is true for the regular 'between' or false for "val in (a:b)" (open range), orfalse if the endpoint is not included
        /// </param>
        /// <param name="highEndpointIncluded">indicates whether the high endpoint is included</param>
        /// <param name="notBetween">is true for 'not between' or 'not in (a:b), or false for a regular between</param>
        public ExprBetweenNodeImpl(
            bool lowEndpointIncluded,
            bool highEndpointIncluded,
            bool notBetween)
        {
            IsLowEndpointIncluded = lowEndpointIncluded;
            IsHighEndpointIncluded = highEndpointIncluded;
            IsNotBetween = notBetween;
        }

        public bool IsConstantResult => false;

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(_forge);
                return _forge.ExprEvaluator;
            }
        }

        /// <summary>
        ///     Returns true if the low endpoint is included, false if not
        /// </summary>
        /// <returns>indicator if endppoint is included</returns>
        public bool IsLowEndpointIncluded { get; }

        /// <summary>
        ///     Returns true if the high endpoint is included, false if not
        /// </summary>
        /// <returns>indicator if endppoint is included</returns>
        public bool IsHighEndpointIncluded { get; }

        /// <summary>
        ///     Returns true for inverted range, or false for regular (openn/close/half-open/half-closed) ranges.
        /// </summary>
        /// <returns>true for not betwene, false for between</returns>
        public bool IsNotBetween { get; }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length != 3) {
                throw new ExprValidationException("The Between operator requires exactly 3 child expressions");
            }

            // Must be either numeric or string
            var forges = ExprNodeUtilityQuery.GetForges(ChildNodes);
            var evalForge = forges[0];
            var evalType = evalForge.EvaluationType.GetBoxedType();
            if (evalType.IsNullTypeSafe()) {
                throw new ExprValidationException("Null value not allowed in between-clause");
            }
            
            var startForge = forges[1];
            var startType = startForge.EvaluationType;
            var endForge = forges[2];
            var endType = endForge.EvaluationType;

            Type compareType;
            var isAlwaysFalse = false;
            ExprBetweenComp computer = null;
            if (startType.IsNullTypeSafe() || endType.IsNullTypeSafe()) {
                isAlwaysFalse = true;
            }
            else {
                if (evalType != typeof(string)
                    || startType != typeof(string)
                    || endType != typeof(string)) {

                    ExprNodeUtilityValidate.ValidateReturnsNumeric(evalForge);
                    ExprNodeUtilityValidate.ValidateReturnsNumeric(startForge);
                    ExprNodeUtilityValidate.ValidateReturnsNumeric(endForge);
                }

                var intermedType = evalType.GetCompareToCoercionType(startType);
                compareType = intermedType.GetCompareToCoercionType(endType);
                computer = MakeComputer(compareType, evalType, startType, endType);
            }

            _forge = new ExprBetweenNodeForge(this, computer, isAlwaysFalse);
            return null;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            var other = node as ExprBetweenNodeImpl;
            return other?.IsNotBetween == IsNotBetween;
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;

        public override ExprForge Forge {
            get {
                CheckValidated(_forge);
                return _forge;
            }
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            IList<ExprNode> children = ChildNodes;
            using (var enumerator = children.GetEnumerator()) {
                if (IsLowEndpointIncluded && IsHighEndpointIncluded) {
                    enumerator.Advance().ToEPL(writer, Precedence, flags);
                    if (IsNotBetween) {
                        writer.Write(" not between ");
                    }
                    else {
                        writer.Write(" between ");
                    }

                    enumerator.Advance().ToEPL(writer, Precedence, flags);
                    writer.Write(" and ");
                    enumerator.Advance().ToEPL(writer, Precedence, flags);
                }
                else {
                    enumerator.Advance().ToEPL(writer, Precedence, flags);
                    writer.Write(" in ");
                    if (IsLowEndpointIncluded) {
                        writer.Write('[');
                    }
                    else {
                        writer.Write('(');
                    }

                    enumerator.Advance().ToEPL(writer, Precedence, flags);
                    writer.Write(':');
                    enumerator.Advance().ToEPL(writer, Precedence, flags);
                    if (IsHighEndpointIncluded) {
                        writer.Write(']');
                    }
                    else {
                        writer.Write(')');
                    }
                }
            }
        }

        private ExprBetweenComp MakeComputer(
            Type compareType,
            Type valueType,
            Type lowType,
            Type highType)
        {
            ExprBetweenComp computer;

            if (compareType == typeof(string)) {
                computer = new ExprBetweenCompString(IsLowEndpointIncluded, IsHighEndpointIncluded);
            }
            else if (compareType.IsDecimal()) {
                computer = new ExprBetweenCompDecimal(
                    IsLowEndpointIncluded,
                    IsHighEndpointIncluded,
                    valueType,
                    lowType,
                    highType);
            }
            else if (compareType.IsBigInteger()) {
                computer = new ExprBetweenCompBigInteger(
                    IsLowEndpointIncluded,
                    IsHighEndpointIncluded,
                    valueType,
                    lowType,
                    highType);
            }
            else if (compareType == typeof(long?)) {
                computer = new ExprBetweenCompLong(IsLowEndpointIncluded, IsHighEndpointIncluded);
            }
            else {
                computer = new ExprBetweenCompDouble(IsLowEndpointIncluded, IsHighEndpointIncluded);
            }

            return computer;
        }

        public interface ExprBetweenComp
        {
            bool IsBetween(
                object value,
                object lower,
                object upper);

            CodegenExpression CodegenNoNullCheck(
                CodegenExpression value,
                Type valueType,
                CodegenExpression lower,
                Type lowerType,
                CodegenExpression higher,
                Type higherType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope);
        }

        internal class ExprBetweenCompString : ExprBetweenComp
        {
            private readonly bool _isHighIncluded;
            private readonly bool _isLowIncluded;

            public ExprBetweenCompString(
                bool lowIncluded,
                bool isHighIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = isHighIncluded;
            }

            public bool IsBetween(
                object value,
                object lower,
                object upper)
            {
                if (value == null || lower == null || upper == null) {
                    return false;
                }

                var valueStr = (string) value;
                var lowerStr = (string) lower;
                var upperStr = (string) upper;

                if (string.Compare(upperStr, lowerStr, StringComparison.Ordinal) < 0) {
                    var temp = upperStr;
                    upperStr = lowerStr;
                    lowerStr = temp;
                }

                if (string.Compare(valueStr, lowerStr, StringComparison.Ordinal) < 0) {
                    return false;
                }

                if (string.Compare(valueStr, upperStr, StringComparison.Ordinal) > 0) {
                    return false;
                }

                if (!_isLowIncluded) {
                    if (valueStr.Equals(lowerStr)) {
                        return false;
                    }
                }

                if (!_isHighIncluded) {
                    if (valueStr.Equals(upperStr)) {
                        return false;
                    }
                }

                return true;
            }

            public CodegenExpression CodegenNoNullCheck(
                CodegenExpression value,
                Type valueType,
                CodegenExpression lower,
                Type lowerType,
                CodegenExpression higher,
                Type higherType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                var block = codegenMethodScope.MakeChild(typeof(bool), typeof(ExprBetweenCompString), codegenClassScope)
                    .AddParam(typeof(string), "value")
                    .AddParam(typeof(string), "lower")
                    .AddParam(typeof(string), "upper")
                    .Block
                    .IfCondition(Relational(ExprDotMethod(Ref("upper"), "CompareTo", Ref("lower")), LT, Constant(0)))
                    .DeclareVar<string>("temp", Ref("upper"))
                    .AssignRef("upper", Ref("lower"))
                    .AssignRef("lower", Ref("temp"))
                    .BlockEnd()
                    .IfCondition(Relational(ExprDotMethod(Ref("value"), "CompareTo", Ref("lower")), LT, Constant(0)))
                    .BlockReturn(ConstantFalse())
                    .IfCondition(Relational(ExprDotMethod(Ref("value"), "CompareTo", Ref("upper")), GT, Constant(0)))
                    .BlockReturn(ConstantFalse());
                if (!_isLowIncluded) {
                    block.IfCondition(StaticMethod<object>("Equals", Ref("value"), Ref("lower"))).BlockReturn(ConstantFalse());
                }

                if (!_isHighIncluded) {
                    block.IfCondition(StaticMethod<object>("Equals", Ref("value"), Ref("upper"))).BlockReturn(ConstantFalse());
                }

                var method = block.MethodReturn(ConstantTrue());
                return LocalMethod(method, value, lower, higher);
            }

            public bool IsEqualsEndpoint(
                object value,
                object endpoint)
            {
                return value.Equals(endpoint);
            }
        }

        internal class ExprBetweenCompDouble : ExprBetweenComp
        {
            private readonly bool _isHighIncluded;
            private readonly bool _isLowIncluded;

            public ExprBetweenCompDouble(
                bool lowIncluded,
                bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(
                object value,
                object lower,
                object upper)
            {
                if (value == null || lower == null || upper == null) {
                    return false;
                }

                var valueD = value.AsDouble();
                var lowerD = lower.AsDouble();
                var upperD = upper.AsDouble();

                if (lowerD > upperD) {
                    var temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD) {
                    if (valueD < upperD) {
                        return true;
                    }

                    if (_isHighIncluded) {
                        return valueD == upperD;
                    }

                    return false;
                }

                if (_isLowIncluded && valueD == lowerD) {
                    return true;
                }

                return false;
            }

            public CodegenExpression CodegenNoNullCheck(
                CodegenExpression value,
                Type valueType,
                CodegenExpression lower,
                Type lowerType,
                CodegenExpression higher,
                Type higherType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                var block = codegenMethodScope.MakeChild(typeof(bool), typeof(ExprBetweenCompDouble), codegenClassScope)
                    .AddParam(typeof(double), "value")
                    .AddParam(typeof(double), "lower")
                    .AddParam(typeof(double), "upper")
                    .Block
                    .IfCondition(Relational(Ref("lower"), GT, Ref("upper")))
                    .DeclareVar<double>("temp", Ref("upper"))
                    .AssignRef("upper", Ref("lower"))
                    .AssignRef("lower", Ref("temp"))
                    .BlockEnd();
                var ifValueGtLower = block.IfCondition(Relational(Ref("value"), GT, Ref("lower")));
                {
                    ifValueGtLower.IfCondition(Relational(Ref("value"), LT, Ref("upper"))).BlockReturn(ConstantTrue());
                    if (_isHighIncluded) {
                        ifValueGtLower.BlockReturn(EqualsIdentity(Ref("value"), Ref("upper")));
                    }
                    else {
                        ifValueGtLower.BlockReturn(ConstantFalse());
                    }
                }
                CodegenMethod method;
                if (_isLowIncluded) {
                    method = block.MethodReturn(EqualsIdentity(Ref("value"), Ref("lower")));
                }
                else {
                    method = block.MethodReturn(ConstantFalse());
                }

                return LocalMethod(method, value, lower, higher);
            }
        }

        internal class ExprBetweenCompLong : ExprBetweenComp
        {
            private readonly bool _isHighIncluded;
            private readonly bool _isLowIncluded;

            public ExprBetweenCompLong(
                bool lowIncluded,
                bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(
                object value,
                object lower,
                object upper)
            {
                if (value == null || lower == null || upper == null) {
                    return false;
                }

                var valueD = value.AsInt64();
                var lowerD = lower.AsInt64();
                var upperD = upper.AsInt64();

                if (lowerD > upperD) {
                    var temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD) {
                    if (valueD < upperD) {
                        return true;
                    }

                    if (_isHighIncluded) {
                        return valueD == upperD;
                    }

                    return false;
                }

                if (_isLowIncluded && valueD == lowerD) {
                    return true;
                }

                return false;
            }

            public CodegenExpression CodegenNoNullCheck(
                CodegenExpression value,
                Type valueType,
                CodegenExpression lower,
                Type lowerType,
                CodegenExpression higher,
                Type higherType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                var block = codegenMethodScope.MakeChild(typeof(bool), typeof(ExprBetweenCompLong), codegenClassScope)
                    .AddParam(typeof(long), "value")
                    .AddParam(typeof(long), "lower")
                    .AddParam(typeof(long), "upper")
                    .Block
                    .IfCondition(Relational(Ref("lower"), GT, Ref("upper")))
                    .DeclareVar<long>("temp", Ref("upper"))
                    .AssignRef("upper", Ref("lower"))
                    .AssignRef("lower", Ref("temp"))
                    .BlockEnd();
                var ifValueGtLower = block.IfCondition(Relational(Ref("value"), GT, Ref("lower")));
                {
                    ifValueGtLower.IfCondition(Relational(Ref("value"), LT, Ref("upper"))).BlockReturn(ConstantTrue());
                    if (_isHighIncluded) {
                        ifValueGtLower.BlockReturn(EqualsIdentity(Ref("value"), Ref("upper")));
                    }
                    else {
                        ifValueGtLower.BlockReturn(ConstantFalse());
                    }
                }
                CodegenMethod method;
                if (_isLowIncluded) {
                    method = block.MethodReturn(EqualsIdentity(Ref("value"), Ref("lower")));
                }
                else {
                    method = block.MethodReturn(ConstantFalse());
                }

                return LocalMethod(method, value, lower, higher);
            }
        }

        internal class ExprBetweenCompDecimal : ExprBetweenComp
        {
            private readonly bool _isHighIncluded;
            private readonly bool _isLowIncluded;

            public ExprBetweenCompDecimal(
                bool lowIncluded,
                bool highIncluded,
                Type valueType,
                Type lowerType,
                Type upperType)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(
                object valueUncast,
                object lowerUncast,
                object upperUncast)
            {
                if (valueUncast == null || lowerUncast == null || upperUncast == null) {
                    return false;
                }

                var value = valueUncast.AsDecimal();
                var lower = lowerUncast.AsDecimal();
                var upper = upperUncast.AsDecimal();

                if (lower.CompareTo(upper) > 0) {
                    var temp = upper;
                    upper = lower;
                    lower = temp;
                }

                var valueComparedLower = value.CompareTo(lower);
                if (valueComparedLower > 0) {
                    var valueComparedUpper = value.CompareTo(upper);
                    if (valueComparedUpper < 0) {
                        return true;
                    }

                    return _isHighIncluded && valueComparedUpper == 0;
                }

                if (_isLowIncluded && valueComparedLower == 0) {
                    return true;
                }

                return false;
            }

            public CodegenExpression CodegenNoNullCheck(
                CodegenExpression value,
                Type valueType,
                CodegenExpression lower,
                Type lowerType,
                CodegenExpression higher,
                Type higherType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                var block = codegenMethodScope.MakeChild(typeof(bool), typeof(ExprBetweenCompDouble), codegenClassScope)
                    .AddParam(typeof(decimal), "value")
                    .AddParam(typeof(decimal), "lower")
                    .AddParam(typeof(decimal), "upper")
                    .Block
                    .IfCondition(Relational(Ref("lower"), GT, Ref("upper")))
                    .DeclareVar<decimal>("temp", Ref("upper"))
                    .AssignRef("upper", Ref("lower"))
                    .AssignRef("lower", Ref("temp"))
                    .BlockEnd();
                var ifValueGtLower = block.IfCondition(Relational(Ref("value"), GT, Ref("lower")));
                {
                    ifValueGtLower.IfCondition(Relational(Ref("value"), LT, Ref("upper"))).BlockReturn(ConstantTrue());
                    if (_isHighIncluded) {
                        ifValueGtLower.BlockReturn(EqualsIdentity(Ref("value"), Ref("upper")));
                    }
                    else {
                        ifValueGtLower.BlockReturn(ConstantFalse());
                    }
                }
                CodegenMethod method;
                if (_isLowIncluded) {
                    method = block.MethodReturn(EqualsIdentity(Ref("value"), Ref("lower")));
                }
                else {
                    method = block.MethodReturn(ConstantFalse());
                }

                return LocalMethod(method, value, lower, higher);
            }
        }

        internal class ExprBetweenCompBigInteger : ExprBetweenComp
        {
            private readonly bool _isHighIncluded;
            private readonly bool _isLowIncluded;
            private readonly BigIntegerCoercer _numberCoercerLower;
            private readonly BigIntegerCoercer _numberCoercerUpper;
            private readonly BigIntegerCoercer _numberCoercerValue;

            public ExprBetweenCompBigInteger(
                bool lowIncluded,
                bool highIncluded,
                Type valueType,
                Type lowerType,
                Type upperType)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;

                _numberCoercerLower = SimpleNumberCoercerFactory.GetCoercerBigInteger(lowerType);
                _numberCoercerUpper = SimpleNumberCoercerFactory.GetCoercerBigInteger(upperType);
                _numberCoercerValue = SimpleNumberCoercerFactory.GetCoercerBigInteger(valueType);
            }

            public bool IsBetween(
                object value,
                object lower,
                object upper)
            {
                if (value == null || lower == null || upper == null) {
                    return false;
                }

                var valueD = _numberCoercerValue.CoerceBoxedBigInt(value);
                var lowerD = _numberCoercerLower.CoerceBoxedBigInt(lower);
                var upperD = _numberCoercerUpper.CoerceBoxedBigInt(upper);

                if (lowerD.CompareTo(upperD) > 0) {
                    var temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD.CompareTo(lowerD) > 0) {
                    if (valueD.CompareTo(upperD) < 0) {
                        return true;
                    }

                    if (_isHighIncluded) {
                        return valueD.Equals(upperD);
                    }

                    return false;
                }

                if (_isLowIncluded && valueD.Equals(lowerD)) {
                    return true;
                }

                return false;
            }

            public CodegenExpression CodegenNoNullCheck(
                CodegenExpression value,
                Type valueType,
                CodegenExpression lower,
                Type lowerType,
                CodegenExpression higher,
                Type higherType,
                CodegenMethodScope codegenMethodScope,
                CodegenClassScope codegenClassScope)
            {
                var block = codegenMethodScope
                    .MakeChild(typeof(bool), typeof(ExprBetweenCompBigInteger), codegenClassScope)
                    .AddParam(typeof(BigInteger), "value")
                    .AddParam(typeof(BigInteger), "lower")
                    .AddParam(typeof(BigInteger), "upper")
                    .Block
                    .IfRefNullReturnFalse("value")
                    .IfRefNullReturnFalse("lower")
                    .IfRefNullReturnFalse("upper")
                    .IfCondition(Relational(ExprDotMethod(Ref("lower"), "CompareTo", Ref("upper")), GT, Constant(0)))
                    .DeclareVar<BigInteger>("temp", Ref("upper"))
                    .AssignRef("upper", Ref("lower"))
                    .AssignRef("lower", Ref("temp"))
                    .BlockEnd();
                var ifValueGtLower = block.IfCondition(
                    Relational(ExprDotMethod(Ref("value"), "CompareTo", Ref("lower")), GT, Constant(0)));
                {
                    ifValueGtLower
                        .IfCondition(
                            Relational(ExprDotMethod(Ref("value"), "CompareTo", Ref("upper")), LT, Constant(0)))
                        .BlockReturn(ConstantTrue());
                    if (_isHighIncluded) {
                        ifValueGtLower.BlockReturn(StaticMethod<object>("Equals", Ref("value"), Ref("upper")));
                    }
                    else {
                        ifValueGtLower.BlockReturn(ConstantFalse());
                    }
                }
                CodegenMethod method;
                if (_isLowIncluded) {
                    method = block.MethodReturn(StaticMethod<object>("Equals", Ref("value"), Ref("lower")));
                }
                else {
                    method = block.MethodReturn(ConstantFalse());
                }

                var valueCoerced = _numberCoercerValue.CoerceBoxedBigIntCodegen(value, valueType);
                var lowerCoerced = _numberCoercerValue.CoerceBoxedBigIntCodegen(lower, lowerType);
                var higherCoerced = _numberCoercerValue.CoerceBoxedBigIntCodegen(higher, higherType);
                return LocalMethod(method, valueCoerced, lowerCoerced, higherCoerced);
            }
        }
    }
} // end of namespace