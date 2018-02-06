///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents a simple Math (+/-/divide/*) in a filter expression tree.
    /// </summary>
    [Serializable]
    public class ExprMathNode : ExprNodeBase, ExprEvaluator
    {
        private readonly MathArithTypeEnum _mathArithTypeEnum;
        private readonly bool _isIntegerDivision;
        private readonly bool _isDivisionByZeroReturnsNull;
    
        [NonSerialized] private MathArithTypeEnumExtensions.Computer _arithTypeEnumComputer;
        private Type _resultType;
        [NonSerialized] private ExprEvaluator _evaluatorLeft;
        [NonSerialized] private ExprEvaluator _evaluatorRight;
        /// <summary>Ctor. </summary>
        /// <param name="mathArithTypeEnum">type of math</param>
        /// <param name="isIntegerDivision">false for division returns double, true for using Java-standard integer division</param>
        /// <param name="isDivisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
        public ExprMathNode(MathArithTypeEnum mathArithTypeEnum, bool isIntegerDivision, bool isDivisionByZeroReturnsNull)
        {
            _mathArithTypeEnum = mathArithTypeEnum;
            _isIntegerDivision = isIntegerDivision;
            _isDivisionByZeroReturnsNull = isDivisionByZeroReturnsNull;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count != 2)
            {
                throw new ExprValidationException("Arithmatic node must have 2 parameters");
            }
    
            foreach (ExprNode child in ChildNodes)
            {
                var childType = child.ExprEvaluator.ReturnType;
                if (!childType.IsNumeric())
                {
                    throw new ExprValidationException(
                        string.Format("Implicit conversion from datatype '{0}' to numeric is not allowed", Name.Clean(childType)));
                }
            }
    
            // Determine result type, set up compute function
            _evaluatorLeft = ChildNodes[0].ExprEvaluator;
            _evaluatorRight = ChildNodes[1].ExprEvaluator;

            var childTypeOne = _evaluatorLeft.ReturnType;
            var childTypeTwo = _evaluatorRight.ReturnType;
    
            if ((childTypeOne == typeof(short) || childTypeOne == typeof(short?)) &&
                (childTypeTwo == typeof(short) || childTypeTwo == typeof(short?)))
            {
                _resultType = typeof (int?);
            }
            else if ((childTypeOne == typeof(byte) || childTypeOne == typeof(byte?)) &&
                     (childTypeTwo == typeof(byte) || childTypeTwo == typeof(byte?)))
            {
                _resultType = typeof (int?);
            }
            else if (childTypeOne == childTypeTwo)
            {
                _resultType = childTypeTwo.GetBoxedType();
            }
            else
            {
                _resultType = childTypeOne.GetArithmaticCoercionType(childTypeTwo);
            }
    
            if ((_mathArithTypeEnum == MathArithTypeEnum.DIVIDE) && (!_isIntegerDivision))
            {
                if (_resultType != typeof(decimal?))
                {
                    _resultType = typeof(double?);
                }
            }

            _arithTypeEnumComputer = _mathArithTypeEnum.GetComputer(_resultType, childTypeOne, childTypeTwo, _isIntegerDivision, _isDivisionByZeroReturnsNull, validationContext.EngineImportService.DefaultMathContext);

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
                i => i.QExprMath(this, _mathArithTypeEnum.GetExpressionText()),
                i => i.AExprMath(result.Value)))
            {
                result.Value = EvaluateInternal(evaluateParams);
                return result.Value;
            }
        }

        private object EvaluateInternal(EvaluateParams evaluateParams)
        {
            var valueChildOne = _evaluatorLeft.Evaluate(evaluateParams);
            if (valueChildOne == null)
            {
                return null;
            }

            var valueChildTwo = _evaluatorRight.Evaluate(evaluateParams);
            if (valueChildTwo == null)
            {
                return null;
            }
    
            // arithTypeEnumComputer is initialized by validation
            return _arithTypeEnumComputer.Invoke(valueChildOne, valueChildTwo);
        }
    
        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            writer.Write(_mathArithTypeEnum.GetExpressionText());
            ChildNodes[1].ToEPL(writer, Precedence);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get
            {
                if (_mathArithTypeEnum == MathArithTypeEnum.MULTIPLY ||
                    _mathArithTypeEnum == MathArithTypeEnum.DIVIDE ||
                    _mathArithTypeEnum == MathArithTypeEnum.MODULO)
                {
                    return ExprPrecedenceEnum.MULTIPLY;
                }
                else
                {
                    return ExprPrecedenceEnum.ADDITIVE;
                }
            }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var other = node as ExprMathNode;
            if (other == null)
            {
                return false;
            }
    
            return other._mathArithTypeEnum == _mathArithTypeEnum;
        }

        /// <summary>Returns the type of math. </summary>
        /// <value>math type</value>
        public MathArithTypeEnum MathArithTypeEnum
        {
            get { return _mathArithTypeEnum; }
        }
    }
}
