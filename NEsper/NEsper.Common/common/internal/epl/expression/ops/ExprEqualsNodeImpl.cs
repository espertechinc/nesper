///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents an equals (=) comparator in a filter expression tree.
    /// </summary>
    [Serializable]
    public class ExprEqualsNodeImpl : ExprNodeBase , ExprEqualsNode {

	    private readonly bool isNotEquals;
	    private readonly bool isIs;

	    [NonSerialized] private ExprEqualsNodeForge forge;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="isNotEquals">true if this is a (!=) not equals rather then equals, false if its a '=' equals</param>
	    /// <param name="isIs">true when "is" or "is not" (instead of = or &amp;lt;&amp;gt;)</param>
	    public ExprEqualsNodeImpl(bool isNotEquals, bool isIs) {
	        this.isNotEquals = isNotEquals;
	        this.isIs = isIs;
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
	        // Must have 2 child nodes
	        if (this.ChildNodes.Length != 2) {
	            throw new ExprValidationException("Invalid use of equals, expecting left-hand side and right-hand side but received " + this.ChildNodes.Length + " expressions");
	        }

	        // Must be the same boxed type returned by expressions under this
	        ExprNode lhs = ChildNodes[0];
	        ExprNode rhs = ChildNodes[1];
	        Type typeOne = Boxing.GetBoxedType(lhs.Forge.EvaluationType);
	        Type typeTwo = Boxing.GetBoxedType(rhs.Forge.EvaluationType);

	        // Null constants can be compared for any type
	        if (typeOne == null || typeTwo == null) {
	            forge = new ExprEqualsNodeForgeNC(this);
	            return null;
	        }

	        if (typeOne.Equals(typeTwo) || typeOne.IsAssignableFrom(typeTwo)) {
	            forge = new ExprEqualsNodeForgeNC(this);
	            return null;
	        }

	        // Get the common type such as Bool, String or Double and Long
	        Type coercionType;
	        try {
	            coercionType = TypeHelper.GetCompareToCoercionType(typeOne, typeTwo);
	        } catch (CoercionException ex) {
	            throw new ExprValidationException("Implicit conversion from datatype '" +
	                    typeTwo.GetSimpleName() +
	                    "' to '" +
	                    typeOne.GetSimpleName() +
	                    "' is not allowed");
	        }

	        // Check if we need to coerce
	        if ((coercionType == Boxing.GetBoxedType(typeOne)) &&
	                (coercionType == Boxing.GetBoxedType(typeTwo))) {
	            forge = new ExprEqualsNodeForgeNC(this);
	        } else {
	            if (!TypeHelper.IsNumeric(coercionType)) {
	                throw new ExprValidationException("Cannot convert datatype '" + coercionType.Name + "' to a value that fits both type '" + typeOne.Name + "' and type '" + typeTwo.Name + "'");
	            }
	            var numberCoercerLHS = CoercerFactory.GetCoercer(typeOne, coercionType);
	            var numberCoercerRHS = CoercerFactory.GetCoercer(typeTwo, coercionType);
	            forge = new ExprEqualsNodeForgeCoercion(this, numberCoercerLHS, numberCoercerRHS);
	        }
	        return null;
	    }

	    public bool IsConstantResult
	    {
	        get => false;
	    }

        public IDictionary<string, object> EventType {
            get { return null; }
        }

        public override void ToPrecedenceFreeEPL(StringWriter writer) {
	        this.ChildNodes[0].ToEPL(writer, Precedence);
	        if (isIs) {
	            writer.Write(" is ");
	            if (isNotEquals) {
	                writer.Write("not ");
	            }
	        } else {
	            if (!isNotEquals) {
	                writer.Write("=");
	            } else {
	                writer.Write("!=");
	            }
	        }
	        this.ChildNodes[1].ToEPL(writer, Precedence);
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get => ExprPrecedenceEnum.EQUALS;
	    }

	    public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) {
	        if (!(node is ExprEqualsNode)) {
	            return false;
	        }

	        ExprEqualsNode other = (ExprEqualsNode) node;
	        return Equals(other.IsNotEquals, this.IsNotEquals);
	    }

	    public bool IsNotEquals
	    {
	        get => isNotEquals;
	    }

	    public bool IsIs
	    {
	        get => isIs;
	    }
	}
} // end of namespace