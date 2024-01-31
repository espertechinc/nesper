///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    /// <summary>
    ///     Represents the COALESCE(a,b,...) function is an expression tree.
    /// </summary>
    public class ExprCoalesceNode : ExprNodeBase
    {
        [JsonIgnore]
        [NonSerialized]
        private ExprCoalesceNodeForge _forge;

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

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length < 2) {
                throw new ExprValidationException("Coalesce node must have at least 2 parameters");
            }

            // get child expression types
            var childTypes = new Type[ChildNodes.Length];
            for (var i = 0; i < ChildNodes.Length; i++) {
                childTypes[i] = ChildNodes[i].Forge.EvaluationType;
            }

            // determine coercion type
            Type resultType;
            try {
                resultType = TypeHelper.GetCommonCoercionType(childTypes);
            }
            catch (CoercionException ex) {
                throw new ExprValidationException("Implicit conversion not allowed: " + ex.Message);
            }

            // determine which child nodes need numeric coercion
            var isNumericCoercion = new bool[ChildNodes.Length];
            for (var i = 0; i < ChildNodes.Length; i++) {
                var node = ChildNodes[i];
                var forgeEvaluationType = node.Forge.EvaluationType;
                if (forgeEvaluationType != null &&
                    forgeEvaluationType.GetBoxedType() != resultType &&
                    resultType != null) {
                    if (!resultType.IsTypeNumeric()) {
                        throw new ExprValidationException(
                            "Implicit conversion from datatype '" +
                            resultType.CleanName() +
                            "' to " +
                            forgeEvaluationType.CleanName() +
                            " is not allowed");
                    }

                    isNumericCoercion[i] = true;
                }
            }

            _forge = new ExprCoalesceNodeForge(this, resultType, isNumericCoercion);
            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ExprNodeUtilityPrint.ToExpressionStringWFunctionName("coalesce", ChildNodes, writer);
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprCoalesceNode)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace