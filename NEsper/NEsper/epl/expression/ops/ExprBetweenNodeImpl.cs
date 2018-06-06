///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents the between-clause function in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprBetweenNodeImpl
        : ExprNodeBase
        , ExprEvaluator
        , ExprBetweenNode
    {
        private readonly bool _isLowEndpointIncluded;
        private readonly bool _isHighEndpointIncluded;
        private readonly bool _isNotBetween;

        private bool _isAlwaysFalse;
        [NonSerialized]
        private IExprBetweenComp _computer;
        [NonSerialized]
        private ExprEvaluator[] _evaluators;

        /// <summary>Ctor. </summary>
        /// <param name="lowEndpointIncluded">is true for the regular 'between' or false for "val in (a:b)" (open range), orfalse if the endpoint is not included </param>
        /// <param name="highEndpointIncluded">indicates whether the high endpoint is included</param>
        /// <param name="notBetween">is true for 'not between' or 'not in (a:b), or false for a regular between</param>
        public ExprBetweenNodeImpl(bool lowEndpointIncluded, bool highEndpointIncluded, bool notBetween)
        {
            _isLowEndpointIncluded = lowEndpointIncluded;
            _isHighEndpointIncluded = highEndpointIncluded;
            _isNotBetween = notBetween;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        /// <summary>Returns true if the low endpoint is included, false if not </summary>
        /// <value>indicator if endppoint is included</value>
        public bool IsLowEndpointIncluded
        {
            get { return _isLowEndpointIncluded; }
        }

        /// <summary>Returns true if the high endpoint is included, false if not </summary>
        /// <value>indicator if endppoint is included</value>
        public bool IsHighEndpointIncluded
        {
            get { return _isHighEndpointIncluded; }
        }

        /// <summary>Returns true for inverted range, or false for regular (openn/close/half-open/half-closed) ranges. </summary>
        /// <value>true for not betwene, false for between</value>
        public bool IsNotBetween
        {
            get { return _isNotBetween; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (((ExprNode)this).ChildNodes.Count != 3)
            {
                throw new ExprValidationException("The Between operator requires exactly 3 child expressions");
            }

            // Must be either numeric or string
            _evaluators = ExprNodeUtility.GetEvaluators(((ExprNode)this).ChildNodes);
            Type typeOne = _evaluators[0].ReturnType.GetBoxedType();
            Type typeTwo = _evaluators[1].ReturnType.GetBoxedType();
            Type typeThree = _evaluators[2].ReturnType.GetBoxedType();

            if (typeOne == null)
            {
                throw new ExprValidationException("Null value not allowed in between-clause");
            }

            Type compareType;
            if ((typeTwo == null) || (typeThree == null))
            {
                _isAlwaysFalse = true;
            }
            else
            {
                if ((typeOne != typeof(String)) || (typeTwo != typeof(String)) || (typeThree != typeof(String)))
                {
                    if (!typeOne.IsNumeric())
                    {
                        throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to numeric is not allowed", Name.Clean(typeOne)));
                    }
                    if (!typeTwo.IsNumeric())
                    {
                        throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to numeric is not allowed", Name.Clean(typeTwo)));
                    }
                    if (!typeThree.IsNumeric())
                    {
                        throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to numeric is not allowed", Name.Clean(typeThree)));
                    }
                }

                Type intermedType = typeOne.GetCompareToCoercionType(typeTwo);
                compareType = intermedType.GetCompareToCoercionType(typeThree);
                _computer = MakeComputer(compareType, typeOne, typeTwo, typeThree);
            }

            return null;
        }

        public Type ReturnType
        {
            get { return typeof(bool?); }
        }

        public Object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprBetween(this); }

            if (!_isAlwaysFalse)
            {
                // Evaluate first child which is the base value to compare to
                var value = _evaluators[0].Evaluate(evaluateParams);
                if (value != null)
                {
                    var lower = _evaluators[1].Evaluate(evaluateParams);
                    var higher = _evaluators[2].Evaluate(evaluateParams);
                    var result = _computer.IsBetween(value, lower, higher);
                    result = _isNotBetween ? result == false : result;
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprBetween(result); }
                    return result;
                }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprBetween(false); }

            return false;
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var other = node as ExprBetweenNodeImpl;
            if (other == null)
            {
                return false;
            }

            return other._isNotBetween == _isNotBetween;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            IEnumerator<ExprNode> it = ((ExprNode)this).ChildNodes.Cast<ExprNode>().GetEnumerator();
            it.MoveNext();
            it.Current.ToEPL(writer, Precedence);
            if (_isNotBetween)
            {
                writer.Write(" not between ");
            }
            else
            {
                writer.Write(" between ");
            }

            it.MoveNext();
            it.Current.ToEPL(writer, Precedence);
            writer.Write(" and ");
            it.MoveNext();
            it.Current.ToEPL(writer, Precedence);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }

        private IExprBetweenComp MakeComputer(Type compareType, Type valueType, Type lowType, Type highType)
        {

            if (compareType == typeof(String))
            {
                return new ExprBetweenCompString(_isLowEndpointIncluded, _isHighEndpointIncluded);
            }

            if ((compareType == valueType) && (compareType == lowType) && (compareType == highType))
            {
                if (compareType == typeof(double?))
                {
                    return new FastExprBetweenCompDouble(_isLowEndpointIncluded, _isHighEndpointIncluded);
                }
                else if (compareType == typeof(decimal?))
                {
                    return new FastExprBetweenCompDecimal(_isLowEndpointIncluded, _isHighEndpointIncluded);
                }
                else if (compareType == typeof(short?))
                {
                    return new FastExprBetweenCompInt16(_isLowEndpointIncluded, _isHighEndpointIncluded);
                }
                else if (compareType == typeof(int?))
                {
                    return new FastExprBetweenCompInt32(_isLowEndpointIncluded, _isHighEndpointIncluded);
                }
                else if (compareType == typeof(long?))
                {
                    return new FastExprBetweenCompInt64(_isLowEndpointIncluded, _isHighEndpointIncluded);
                }
            }

            if (compareType == typeof(double?))
            {
                return new ExprBetweenCompDouble(_isLowEndpointIncluded, _isHighEndpointIncluded);
            }
            if (compareType == typeof(decimal?))
            {
                return new ExprBetweenCompDecimal(_isLowEndpointIncluded, _isHighEndpointIncluded);
            }
            if (compareType == typeof(short?))
            {
                return new ExprBetweenCompInt16(_isLowEndpointIncluded, _isHighEndpointIncluded);
            }
            if (compareType == typeof(int?))
            {
                return new ExprBetweenCompInt32(_isLowEndpointIncluded, _isHighEndpointIncluded);
            }
            if (compareType == typeof(long?))
            {
                return new ExprBetweenCompInt64(_isLowEndpointIncluded, _isHighEndpointIncluded);
            }

            return new ExprBetweenCompDouble(_isLowEndpointIncluded, _isHighEndpointIncluded);
        }

        private interface IExprBetweenComp
        {
            bool IsBetween(Object value, Object lower, Object upper);
        }

        private class ExprBetweenCompString : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public ExprBetweenCompString(bool lowIncluded, bool isHighIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = isHighIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                String valueStr = (String)value;
                String lowerStr = (String)lower;
                String upperStr = (String)upper;

                if (upperStr.CompareTo(lowerStr) < 0)
                {
                    String temp = upperStr;
                    upperStr = lowerStr;
                    lowerStr = temp;
                }

                if (valueStr.CompareTo(lowerStr) < 0)
                {
                    return false;
                }
                if (valueStr.CompareTo(upperStr) > 0)
                {
                    return false;
                }
                if (!(_isLowIncluded) && valueStr.Equals(lowerStr))
                {
                    return false;
                }

                return (_isHighIncluded) || !valueStr.Equals(upperStr);
            }
        }

        private class ExprBetweenCompDecimal : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public ExprBetweenCompDecimal(bool lowIncluded, bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                var valueD = value.AsDecimal();
                var lowerD = lower.AsDecimal();
                var upperD = upper.AsDecimal();

                if (lowerD > upperD)
                {
                    var temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD)
                {
                    return valueD < upperD || _isHighIncluded && valueD == upperD;
                }

                return (_isLowIncluded) && (valueD == lowerD);
            }
        }

        private class FastExprBetweenCompDecimal : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public FastExprBetweenCompDecimal(bool lowIncluded, bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                var valueD = (decimal)value;
                var lowerD = (decimal)lower;
                var upperD = (decimal)upper;

                if (lowerD > upperD)
                {
                    var temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD)
                {
                    return valueD < upperD || _isHighIncluded && valueD == upperD;
                }

                return (_isLowIncluded) && (valueD == lowerD);
            }
        }

        private class FastExprBetweenCompDouble : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public FastExprBetweenCompDouble(bool lowIncluded, bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                double valueD = (double)value;
                double lowerD = (double)lower;
                double upperD = (double)upper;

                if (lowerD > upperD)
                {
                    double temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD)
                {
                    return valueD < upperD || _isHighIncluded && valueD == upperD;
                }

                return (_isLowIncluded) && (valueD == lowerD);
            }
        }

        private class ExprBetweenCompDouble : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public ExprBetweenCompDouble(bool lowIncluded, bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                double valueD = value.AsDouble();
                double lowerD = lower.AsDouble();
                double upperD = upper.AsDouble();

                if (lowerD > upperD)
                {
                    double temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD)
                {
                    return valueD < upperD || _isHighIncluded && valueD == upperD;
                }

                return (_isLowIncluded) && (valueD == lowerD);
            }
        }

        private class FastExprBetweenCompInt64 : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public FastExprBetweenCompInt64(bool lowIncluded, bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                long valueD = (long)value;
                long lowerD = (long)lower;
                long upperD = (long)upper;

                if (lowerD > upperD)
                {
                    long temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD)
                {
                    return valueD < upperD || _isHighIncluded && valueD == upperD;
                }

                return (_isLowIncluded) && (valueD == lowerD);
            }
        }

        private class ExprBetweenCompInt64 : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public ExprBetweenCompInt64(bool lowIncluded, bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                long valueD = value.AsLong();
                long lowerD = lower.AsLong();
                long upperD = upper.AsLong();

                if (lowerD > upperD)
                {
                    var temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD)
                {
                    return valueD < upperD || _isHighIncluded && valueD == upperD;
                }

                return (_isLowIncluded) && (valueD == lowerD);
            }
        }

        private class FastExprBetweenCompInt32 : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public FastExprBetweenCompInt32(bool lowIncluded, bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                int valueD = (int)value;
                int lowerD = (int)lower;
                int upperD = (int)upper;

                if (lowerD > upperD)
                {
                    int temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD)
                {
                    return valueD < upperD || _isHighIncluded && valueD == upperD;
                }

                return (_isLowIncluded) && (valueD == lowerD);
            }
        }

        private class ExprBetweenCompInt32 : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public ExprBetweenCompInt32(bool lowIncluded, bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                int valueD = value.AsInt();
                int lowerD = lower.AsInt();
                int upperD = upper.AsInt();

                if (lowerD > upperD)
                {
                    int temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD)
                {
                    return valueD < upperD || _isHighIncluded && valueD == upperD;
                }

                return (_isLowIncluded) && (valueD == lowerD);
            }
        }

        private class FastExprBetweenCompInt16 : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public FastExprBetweenCompInt16(bool lowIncluded, bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                short valueD = (short)value;
                short lowerD = (short)lower;
                short upperD = (short)upper;

                if (lowerD > upperD)
                {
                    short temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD)
                {
                    return valueD < upperD || _isHighIncluded && valueD == upperD;
                }

                return (_isLowIncluded) && (valueD == lowerD);
            }
        }

        private class ExprBetweenCompInt16 : IExprBetweenComp
        {
            private readonly bool _isLowIncluded;
            private readonly bool _isHighIncluded;

            public ExprBetweenCompInt16(bool lowIncluded, bool highIncluded)
            {
                _isLowIncluded = lowIncluded;
                _isHighIncluded = highIncluded;
            }

            public bool IsBetween(Object value, Object lower, Object upper)
            {
                if ((value == null) || (lower == null) || ((upper == null)))
                {
                    return false;
                }

                short valueD = value.AsShort();
                short lowerD = lower.AsShort();
                short upperD = upper.AsShort();

                if (lowerD > upperD)
                {
                    short temp = upperD;
                    upperD = lowerD;
                    lowerD = temp;
                }

                if (valueD > lowerD)
                {
                    return valueD < upperD || _isHighIncluded && valueD == upperD;
                }

                return (_isLowIncluded) && (valueD == lowerD);
            }
        }
    }
}
