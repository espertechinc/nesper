///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    /// Represents the MAX(a,b) and MIN(a,b) functions is an expression tree.
    /// </summary>
    public class ExprMinMaxRowNode : ExprNodeBase
    {
        private readonly MinMaxTypeEnum _minMaxTypeEnum;

        [NonSerialized] private ExprMinMaxRowNodeForge _forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="minMaxTypeEnum">type of compare</param>
        public ExprMinMaxRowNode(MinMaxTypeEnum minMaxTypeEnum)
        {
            _minMaxTypeEnum = minMaxTypeEnum;
        }

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

        /// <summary>
        /// Returns the indicator for minimum or maximum.
        /// </summary>
        /// <returns>min/max indicator</returns>
        public MinMaxTypeEnum MinMaxTypeEnum {
            get => _minMaxTypeEnum;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length < 2) {
                throw new ExprValidationException("MinMax node must have at least 2 parameters");
            }

            foreach (var child in ChildNodes) {
                ExprNodeUtilityValidate.ValidateReturnsNumeric(child.Forge);
            }

            // Determine result type, set up compute function
            var childTypeOne = ChildNodes[0].Forge.EvaluationType;
            var childTypeTwo = ChildNodes[1].Forge.EvaluationType;
            var resultType = TypeHelper.GetArithmaticCoercionType(childTypeOne, childTypeTwo);

            for (var i = 2; i < ChildNodes.Length; i++) {
                var childTypeMore = ChildNodes[i].Forge.EvaluationType;
                resultType = TypeHelper.GetArithmaticCoercionType(resultType, childTypeMore);
            }

            _forge = new ExprMinMaxRowNodeForge(this, resultType);

            return null;
        }

        public bool IsConstantResult {
            get => false;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(_minMaxTypeEnum.GetExpressionText());
            writer.Write('(');

            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
            writer.Write(',');
            ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);

            for (var i = 2; i < ChildNodes.Length; i++) {
                writer.Write(',');
                ChildNodes[i].ToEPL(writer, ExprPrecedenceEnum.MINIMUM, flags);
            }

            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.UNARY;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprMinMaxRowNode)) {
                return false;
            }

            var other = (ExprMinMaxRowNode) node;

            if (other._minMaxTypeEnum != _minMaxTypeEnum) {
                return false;
            }

            return true;
        }
    }
} // end of namespace