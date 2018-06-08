///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents the bit-wise operators in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprBitWiseNode
        : ExprNodeBase
        , ExprEvaluator
    {
        private readonly BitWiseOpEnum _bitWiseOpEnum;
        [NonSerialized]
        private BitWiseOpEnumExtensions.Computer _bitWiseOpEnumComputer;
        private Type _returnType;

        [NonSerialized]
        private ExprEvaluator[] _evaluators;

        /// <summary>Ctor. </summary>
        /// <param name="bitWiseOpEnum">type of math</param>
        public ExprBitWiseNode(BitWiseOpEnum bitWiseOpEnum)
        {
            _bitWiseOpEnum = bitWiseOpEnum;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        /// <summary>Returns the bitwise operator. </summary>
        /// <value>operator</value>
        public BitWiseOpEnum BitWiseOpEnum
        {
            get { return _bitWiseOpEnum; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count != 2)
            {
                throw new ExprValidationException("BitWise node must have 2 parameters");
            }

            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
            foreach (var child in _evaluators)
            {
                var childType = child.ReturnType;
                if ((!childType.IsBoolean()) && (!childType.IsNumeric()))
                {
                    throw new ExprValidationException("Invalid datatype for bitwise " +
                            childType.Name + " is not allowed");
                }
            }

            // Determine result type, set up compute function
            var childTypeOne = _evaluators[0].ReturnType;
            var childTypeTwo = _evaluators[1].ReturnType;
            if ((childTypeOne.IsFloatingPointClass()) || (childTypeTwo.IsFloatingPointClass()))
            {
                throw new ExprValidationException("Invalid type for bitwise " + _bitWiseOpEnum.GetComputeDescription() + " operator");
            }
            else
            {
                var childBoxedTypeOne = childTypeOne.GetBoxedType();
                var childBoxedTypeTwo = childTypeTwo.GetBoxedType();
                if (childBoxedTypeOne == childBoxedTypeTwo)
                {
                    _returnType = childBoxedTypeOne;
                    _bitWiseOpEnumComputer = _bitWiseOpEnum.GetComputer(_returnType);
                }
                else
                {
                    throw new ExprValidationException("Bitwise expressions must be of the same type for bitwise " + _bitWiseOpEnum.GetComputeDescription() + " operator");
                }
            }

            return null;
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { return _returnType; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprBitwise(this, _bitWiseOpEnum); }
            var valueChildOne = _evaluators[0].Evaluate(evaluateParams);
            var valueChildTwo = _evaluators[1].Evaluate(evaluateParams);

            if ((valueChildOne == null) || (valueChildTwo == null))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprBitwise(null); }
                return null;
            }

            // bitWiseOpEnumComputer is initialized by validation
            if (InstrumentationHelper.ENABLED)
            {
                var result = _bitWiseOpEnumComputer.Invoke(valueChildOne, valueChildTwo);
                InstrumentationHelper.Get().AExprBitwise(result);
                return result;
            }
            return _bitWiseOpEnumComputer.Invoke(valueChildOne, valueChildTwo);
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprBitWiseNode))
            {
                return false;
            }

            var other = (ExprBitWiseNode)node;
            if (other._bitWiseOpEnum != _bitWiseOpEnum)
            {
                return false;
            }

            return true;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            writer.Write(_bitWiseOpEnum.GetComputeDescription());
            ChildNodes[1].ToEPL(writer, Precedence);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.BITWISE; }
        }
    }
}
