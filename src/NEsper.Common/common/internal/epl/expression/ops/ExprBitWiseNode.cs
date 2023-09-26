///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Represents the bit-wise operators in an expression tree.
    /// </summary>
    public class ExprBitWiseNode : ExprNodeBase
    {
        private readonly BitWiseOpEnum bitWiseOpEnum;

        [NonSerialized] private ExprBitWiseNodeForge forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="bitWiseOpEnum">type of math</param>
        public ExprBitWiseNode(BitWiseOpEnum bitWiseOpEnum)
        {
            this.bitWiseOpEnum = bitWiseOpEnum;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        /// <summary>
        /// Returns the bitwise operator.
        /// </summary>
        /// <value>operator</value>
        public BitWiseOpEnum BitWiseOpEnum => bitWiseOpEnum;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length != 2) {
                throw new ExprValidationException("BitWise node must have 2 parameters");
            }

            var lhsType = ChildNodes[0].Forge.EvaluationType;
            var rhsType = ChildNodes[1].Forge.EvaluationType;
            CheckNumericOrBoolean(lhsType);
            CheckNumericOrBoolean(rhsType);

            var lhsTypeClass = lhsType.GetBoxedType();
            var rhsTypeClass = rhsType.GetBoxedType();

            if (lhsTypeClass.IsFloatingPointClass() || rhsTypeClass.IsFloatingPointClass()) {
                throw new ExprValidationException(
                    "Invalid type for bitwise " + bitWiseOpEnum.ComputeDescription + " operator");
            }

            if (!lhsTypeClass.Equals(rhsTypeClass)) {
                throw new ExprValidationException(
                    "Bitwise expressions must be of the same type for bitwise " +
                    bitWiseOpEnum.ComputeDescription +
                    " operator");
            }

            var computer = bitWiseOpEnum.GetComputer(lhsTypeClass);
            forge = new ExprBitWiseNodeForge(this, lhsTypeClass, computer);
            return null;
        }

        public bool IsConstantResult => false;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprBitWiseNode other)) {
                return false;
            }

            if (other.bitWiseOpEnum != bitWiseOpEnum) {
                return false;
            }

            return true;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            writer.Write(bitWiseOpEnum.ComputeDescription);
            ChildNodes[1].ToEPL(writer, Precedence, flags);
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.BITWISE;

        private void CheckNumericOrBoolean(Type childType)
        {
            if (childType == null || (!childType.IsTypeBoolean() && !childType.IsTypeNumeric())) {
                throw new ExprValidationException(
                    $"Invalid datatype for binary operator, {childType.CleanName()} is not allowed");
            }
        }
    }
} // end of namespace