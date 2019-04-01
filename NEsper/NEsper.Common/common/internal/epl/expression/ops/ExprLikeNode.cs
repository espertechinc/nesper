///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents the like-clause in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprLikeNode : ExprNodeBase {

	    private readonly bool isNot;

	    [NonSerialized] private ExprLikeNodeForge forge;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="not">is true if this is a "not like", or false if just a like</param>
	    public ExprLikeNode(bool not) {
	        this.isNot = not;
	    }

	    public Type EvaluationType
	    {
	        get => typeof(bool?);
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

        public override ExprNode Validate(ExprValidationContext validationContext) {
	        if ((this.ChildNodes.Length != 2) && (this.ChildNodes.Length != 3)) {
	            throw new ExprValidationException("The 'like' operator requires 2 (no escape) or 3 (with escape) child expressions");
	        }

	        // check eval child node - can be String or numeric
	        Type evalChildType = ChildNodes[0].Forge.EvaluationType;
	        bool isNumericValue = TypeHelper.IsNumeric(evalChildType);
	        if ((evalChildType != typeof(string)) && (!isNumericValue)) {
	            throw new ExprValidationException("The 'like' operator requires a String or numeric type left-hand expression");
	        }

	        // check pattern child node
	        Type patternChildType = ChildNodes[1].Forge.EvaluationType;
	        if (patternChildType != typeof(string)) {
	            throw new ExprValidationException("The 'like' operator requires a String-type pattern expression");
	        }
	        bool isConstantPattern = ChildNodes[1].Forge.ForgeConstantType.IsCompileTimeConstant;

	        // check escape character node
	        bool isConstantEscape = true;
	        if (this.ChildNodes.Length == 3) {
	            if (this.ChildNodes[2].Forge.EvaluationType != typeof(string)) {
	                throw new ExprValidationException("The 'like' operator escape parameter requires a character-type value");
	            }
	            isConstantEscape = ChildNodes[2].Forge.ForgeConstantType.IsCompileTimeConstant;
	        }

	        if (isConstantPattern && isConstantEscape) {
	            string patternVal = (string) ChildNodes[1].Forge.ExprEvaluator.Evaluate(null, true, null);
	            if (patternVal == null) {
	                throw new ExprValidationException("The 'like' operator pattern returned null");
	            }
	            string escape = "\\";
	            char? escapeCharacter = null;
	            if (this.ChildNodes.Length == 3) {
	                escape = (string) ChildNodes[2].Forge.ExprEvaluator.Evaluate(null, true, null);
	            }
	            if (escape.Length > 0) {
	                escapeCharacter = escape[0];
	            }
	            LikeUtil likeUtil = new LikeUtil(patternVal, escapeCharacter, false);
	            CodegenExpression likeUtilInit = NewInstance(typeof(LikeUtil), Constant(patternVal), Constant(escapeCharacter), ConstantFalse());
	            forge = new ExprLikeNodeForgeConst(this, isNumericValue, likeUtil, likeUtilInit);
	        } else {
	            forge = new ExprLikeNodeForgeNonconst(this, isNumericValue);
	        }
	        return null;
	    }

	    public Type Type
	    {
	        get => typeof(bool?);
	    }

	    public bool IsConstantResult
	    {
	        get => false;
	    }

	    public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) {
	        if (!(node is ExprLikeNode)) {
	            return false;
	        }

	        ExprLikeNode other = (ExprLikeNode) node;
	        if (this.isNot != other.isNot) {
	            return false;
	        }
	        return true;
	    }

	    public override void ToPrecedenceFreeEPL(StringWriter writer) {
	        this.ChildNodes[0].ToEPL(writer, Precedence);

	        if (isNot) {
	            writer.Write(" not");
	        }

	        writer.Write(" like ");
	        this.ChildNodes[1].ToEPL(writer, Precedence);

	        if (this.ChildNodes.Length == 3) {
	            writer.Write(" escape ");
	            this.ChildNodes[2].ToEPL(writer, Precedence);
	        }

	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;
	    }

	    /// <summary>
	    /// Returns true if this is a "not like", or false if just a like
	    /// </summary>
	    /// <returns>indicator whether negated or not</returns>
	    public bool IsNot
	    {
	        get => isNot;
	    }
	}
} // end of namespace