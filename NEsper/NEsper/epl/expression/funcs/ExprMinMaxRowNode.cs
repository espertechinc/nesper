///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Numerics;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.funcs
{
    /// <summary>
    /// Represents the MAX(a,b) and MIN(a,b) functions is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprMinMaxRowNode : ExprNodeBase, ExprEvaluator
    {
        private readonly MinMaxTypeEnum _minMaxTypeEnum;
        private Type _resultType;
        [NonSerialized] private MinMaxTypeEnumExtensions.Computer _computer;
        [NonSerialized] private ExprEvaluator[] _evaluators;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="minMaxTypeEnum">type of compare</param>
        public ExprMinMaxRowNode(MinMaxTypeEnum minMaxTypeEnum)
        {
            _minMaxTypeEnum = minMaxTypeEnum;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        /// <summary>Returns the indicator for minimum or maximum. </summary>
        /// <value>min/max indicator</value>
        public MinMaxTypeEnum MinMaxTypeEnum
        {
            get { return _minMaxTypeEnum; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count < 2)
            {
                throw new ExprValidationException("MinMax node must have at least 2 parameters");
            }
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);

            foreach (ExprEvaluator child in _evaluators)
            {
                var childType = child.ReturnType;
                if (!childType.IsNumeric())
                {
                    throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to numeric is not allowed", Name.Clean(childType)));
                }
            }

            // Determine result type, set up compute function
            var childTypeOne = _evaluators[0].ReturnType;
            var childTypeTwo = _evaluators[1].ReturnType;
            _resultType = childTypeOne.GetArithmaticCoercionType(childTypeTwo);

            for (int i = 2; i < ChildNodes.Count; i++)
            {
                _resultType = _resultType.GetArithmaticCoercionType(_evaluators[i].ReturnType);
            }

            if (_resultType == typeof(decimal) || _resultType == typeof(decimal?))
            {
                _computer = Equals(_minMaxTypeEnum, MinMaxTypeEnum.MAX)
                    ? MinMaxTypeEnumExtensions.CreateMaxDecimalComputer(_evaluators)
                    : MinMaxTypeEnumExtensions.CreateMinDecimalComputer(_evaluators);
            }
            else if (_resultType == typeof(BigInteger) || _resultType == typeof(BigInteger?))
            {
                _computer = Equals(_minMaxTypeEnum, MinMaxTypeEnum.MAX)
                    ? MinMaxTypeEnumExtensions.CreateMaxBigIntComputer(_evaluators)
                    : MinMaxTypeEnumExtensions.CreateMinBigIntComputer(_evaluators);
            }
            else
            {
                _computer = Equals(_minMaxTypeEnum, MinMaxTypeEnum.MAX)
                    ? MinMaxTypeEnumExtensions.CreateMaxDoubleComputer(_evaluators)
                    : MinMaxTypeEnumExtensions.CreateMinDoubleComputer(_evaluators);
            }

            return null;
        }

        public Type ReturnType
        {
            get { return _resultType; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var result = new Mutable<object>();

            using (Instrument.With(
                i => i.QExprMinMaxRow(this),
                i => i.AExprMinMaxRow(result.Value)))
            {
                result.Value = _computer.Invoke(
                    evaluateParams.EventsPerStream,
                    evaluateParams.IsNewData,
                    evaluateParams.ExprEvaluatorContext);
                if (result.Value != null)
                {
                    result.Value = CoercerFactory.CoerceBoxed(result.Value, _resultType);
                }

                return result.Value;
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_minMaxTypeEnum.GetExpressionText());
            writer.Write('(');

            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(',');
            ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);

            for (int i = 2; i < ChildNodes.Count; i++)
            {
                writer.Write(',');
                ChildNodes[i].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            }

            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var other = node as ExprMinMaxRowNode;
            if (other != null)
            {
                return other._minMaxTypeEnum == _minMaxTypeEnum;
            }

            return false;
        }
    }
}
