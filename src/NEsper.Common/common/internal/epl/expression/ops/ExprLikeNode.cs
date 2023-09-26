///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents the like-clause in an expression tree.
    /// </summary>
    public class ExprLikeNode : ExprNodeBase
    {
        private readonly bool isNot;
        [NonSerialized] private ExprLikeNodeForge forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "not">is true if this is a "not like", or false if just a like</param>
        public ExprLikeNode(bool not)
        {
            isNot = not;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length != 2 && ChildNodes.Length != 3) {
                throw new ExprValidationException(
                    "The 'like' operator requires 2 (no escape) or 3 (with escape) child expressions");
            }

            // check eval child node - can be String or numeric
            var evalChildType = ChildNodes[0].Forge.EvaluationType;
            var isNumericValue = evalChildType.IsTypeNumeric();
            if ((evalChildType != typeof(string)) && (!isNumericValue)) {
                throw new ExprValidationException(
                    "The 'like' operator requires a String or numeric type left-hand expression");
            }

            // check pattern child node
            var patternForge = ChildNodes[1].Forge;
            var patternChildType = patternForge.EvaluationType;
            if (patternChildType != typeof(string)) {
                throw new ExprValidationException("The 'like' operator requires a String-type pattern expression");
            }

            var isConstantPattern = patternForge.ForgeConstantType.IsCompileTimeConstant;
            // check escape character node
            var isConstantEscape = true;
            if (ChildNodes.Length == 3) {
                var escapeForge = ChildNodes[2].Forge;
                if (escapeForge.EvaluationType != typeof(string)) {
                    throw new ExprValidationException(
                        "The 'like' operator escape parameter requires a character-type value");
                }

                isConstantEscape = escapeForge.ForgeConstantType.IsCompileTimeConstant;
            }

            if (isConstantPattern && isConstantEscape) {
                var patternVal = (string)ChildNodes[1].Forge.ExprEvaluator.Evaluate(null, true, null);
                if (patternVal == null) {
                    throw new ExprValidationException("The 'like' operator pattern returned null");
                }

                var escape = "\\";
                char? escapeCharacter = null;
                if (ChildNodes.Length == 3) {
                    escape = (string)ChildNodes[2].Forge.ExprEvaluator.Evaluate(null, true, null);
                }

                if (escape.Length > 0) {
                    escapeCharacter = escape[0];
                }

                var likeUtil = new LikeUtil(patternVal, escapeCharacter, false);
                var likeUtilInit = NewInstance(
                    typeof(LikeUtil),
                    Constant(patternVal),
                    Constant(escapeCharacter),
                    ConstantFalse());
                forge = new ExprLikeNodeForgeConst(this, isNumericValue, likeUtil, likeUtilInit);
            }
            else {
                forge = new ExprLikeNodeForgeNonconst(this, isNumericValue);
            }

            return null;
        }

        public bool IsConstantResult => false;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprLikeNode other)) {
                return false;
            }

            if (isNot != other.isNot) {
                return false;
            }

            return true;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            if (isNot) {
                writer.Write(" not");
            }

            writer.Write(" like ");
            ChildNodes[1].ToEPL(writer, Precedence, flags);
            if (ChildNodes.Length == 3) {
                writer.Write(" escape ");
                ChildNodes[2].ToEPL(writer, Precedence, flags);
            }
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;

        /// <summary>
        /// Returns true if this is a "not like", or false if just a like
        /// </summary>
        /// <returns>indicator whether negated or not</returns>
        public bool IsNot => isNot;

        public Type EvaluationType => typeof(bool?);

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

        public Type Type => typeof(bool?);
    }
} // end of namespace