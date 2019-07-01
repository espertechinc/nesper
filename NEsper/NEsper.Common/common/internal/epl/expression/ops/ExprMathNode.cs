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
    ///     Represents a simple Math (+/-/divide/*) in a filter expression tree.
    /// </summary>
    [Serializable]
    public class ExprMathNode : ExprNodeBase
    {
        private readonly bool isDivisionByZeroReturnsNull;
        private readonly bool isIntegerDivision;

        [NonSerialized] private ExprMathNodeForge forge;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="mathArithTypeEnum">type of math</param>
        /// <param name="isIntegerDivision">false for division returns double, true for using standard integer division</param>
        /// <param name="isDivisionByZeroReturnsNull">false for division-by-zero returns infinity, true for null</param>
        public ExprMathNode(
            MathArithTypeEnum mathArithTypeEnum,
            bool isIntegerDivision,
            bool isDivisionByZeroReturnsNull)
        {
            MathArithTypeEnum = mathArithTypeEnum;
            this.isIntegerDivision = isIntegerDivision;
            this.isDivisionByZeroReturnsNull = isDivisionByZeroReturnsNull;
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

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence {
            get {
                if (MathArithTypeEnum == MathArithTypeEnum.MULTIPLY ||
                    MathArithTypeEnum == MathArithTypeEnum.DIVIDE ||
                    MathArithTypeEnum == MathArithTypeEnum.MODULO) {
                    return ExprPrecedenceEnum.MULTIPLY;
                }

                return ExprPrecedenceEnum.ADDITIVE;
            }
        }

        /// <summary>
        ///     Returns the type of math.
        /// </summary>
        /// <returns>math type</returns>
        public MathArithTypeEnum MathArithTypeEnum { get; }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length != 2) {
                throw new ExprValidationException("Arithmatic node must have 2 parameters");
            }

            foreach (var child in ChildNodes) {
                var childType = child.Forge.EvaluationType;
                if (!childType.IsNumeric()) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" +
                        childType.GetSimpleName() +
                        "' to numeric is not allowed");
                }
            }

            // Determine result type, set up compute function
            var lhs = ChildNodes[0];
            var rhs = ChildNodes[1];
            var lhsType = lhs.Forge.EvaluationType;
            var rhsType = rhs.Forge.EvaluationType;

            Type resultType;
            if ((lhsType == typeof(short) || lhsType == typeof(short?)) &&
                (rhsType == typeof(short) || rhsType == typeof(short?))) {
                resultType = typeof(int?);
            }
            else if ((lhsType == typeof(byte) || lhsType == typeof(byte?)) &&
                     (rhsType == typeof(byte) || rhsType == typeof(byte?))) {
                resultType = typeof(int?);
            }
            else if (lhsType.Equals(rhsType)) {
                resultType = rhsType.GetBoxedType();
            }
            else {
                resultType = lhsType.GetArithmaticCoercionType(rhsType);
            }

            if (MathArithTypeEnum == MathArithTypeEnum.DIVIDE && !isIntegerDivision) {
                if (resultType != typeof(decimal?)) {
                    resultType = typeof(double?);
                }
            }

            var arithTypeEnumComputer = MathArithType.GetComputer(
                MathArithTypeEnum, resultType, lhsType, rhsType, isIntegerDivision, isDivisionByZeroReturnsNull,
                validationContext.ImportService.DefaultMathContext);
            forge = new ExprMathNodeForge(this, arithTypeEnumComputer, resultType);
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            writer.Write(MathArithTypeEnum.GetExpressionText());
            ChildNodes[1].ToEPL(writer, Precedence);
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprMathNode)) {
                return false;
            }

            var other = (ExprMathNode) node;

            if (other.MathArithTypeEnum != MathArithTypeEnum) {
                return false;
            }

            return true;
        }
    }
} // end of namespace