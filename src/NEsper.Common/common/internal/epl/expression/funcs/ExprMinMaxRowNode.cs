///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    /// Represents the MAX(a,b) and MIN(a,b) functions is an expression tree.
    /// </summary>
    public class ExprMinMaxRowNode : ExprNodeBase
    {
        private readonly MinMaxTypeEnum minMaxTypeEnum;

        [JsonIgnore]
        [NonSerialized]
        private ExprMinMaxRowNodeForge forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="minMaxTypeEnum">type of compare</param>
        public ExprMinMaxRowNode(MinMaxTypeEnum minMaxTypeEnum)
        {
            this.minMaxTypeEnum = minMaxTypeEnum;
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
        /// Returns the indicator for minimum or maximum.
        /// </summary>
        /// <returns>min/max indicator</returns>
        public MinMaxTypeEnum MinMaxTypeEnum => minMaxTypeEnum;

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
            var resultType = childTypeOne.GetArithmaticCoercionType(childTypeTwo);

            for (var i = 2; i < ChildNodes.Length; i++) {
                resultType = resultType.GetArithmaticCoercionType(ChildNodes[i].Forge.EvaluationType);
            }

            forge = new ExprMinMaxRowNodeForge(this, resultType);

            return null;
        }

        public bool IsConstantResult => false;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(minMaxTypeEnum.GetExpressionText());
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

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprMinMaxRowNode other)) {
                return false;
            }

            if (other.minMaxTypeEnum != minMaxTypeEnum) {
                return false;
            }

            return true;
        }
    }
} // end of namespace