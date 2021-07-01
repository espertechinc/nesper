///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Represents the bit-wise operators in an expression tree.
    /// </summary>
    public class ExprBitWiseNode : ExprNodeBase
    {
        [NonSerialized] private ExprBitWiseNodeForge _forge;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="bitWiseOpEnum">type of math</param>
        public ExprBitWiseNode(BitWiseOpEnum bitWiseOpEnum)
        {
            BitWiseOpEnum = bitWiseOpEnum;
        }

        /// <summary>
        ///     Returns the bitwise operator.
        /// </summary>
        /// <returns>operator</returns>
        public BitWiseOpEnum BitWiseOpEnum { get; }

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.BITWISE;

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(_forge);
                return _forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(_forge);
                return _forge;
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length != 2) {
                throw new ExprValidationException("BitWise node must have 2 parameters");
            }

            var typeOne = ChildNodes[0].Forge.EvaluationType.GetBoxedType();
            var typeTwo = ChildNodes[1].Forge.EvaluationType.GetBoxedType();
            CheckNumericOrBoolean(typeOne);
            CheckNumericOrBoolean(typeTwo);

            if (typeOne.IsFloatingPointClass() || typeTwo.IsFloatingPointClass()) {
                throw new ExprValidationException(
                    "Invalid type for bitwise " + BitWiseOpEnum.ComputeDescription + " operator");
            }

            if (typeOne != typeTwo) {
                throw new ExprValidationException(
                    "Bitwise expressions must be of the same type for bitwise " +
                    BitWiseOpEnum.ComputeDescription +
                    " operator");
            }

            var computer = BitWiseOpEnum.GetComputer(typeOne);
            _forge = new ExprBitWiseNodeForge(this, typeOne, computer);
            return null;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprBitWiseNode)) {
                return false;
            }

            var other = (ExprBitWiseNode) node;

            if (other.BitWiseOpEnum != BitWiseOpEnum) {
                return false;
            }

            return true;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            writer.Write(BitWiseOpEnum.ComputeDescription);
            ChildNodes[1].ToEPL(writer, Precedence, flags);
        }

        private void CheckNumericOrBoolean(Type childType)
        {
            if (!childType.IsBoolean() && !childType.IsNumeric()) {
                throw new ExprValidationException(
                    "Invalid datatype for binary operator, " +
                    childType.CleanName() +
                    " is not allowed");
            }
        }
    }
} // end of namespace