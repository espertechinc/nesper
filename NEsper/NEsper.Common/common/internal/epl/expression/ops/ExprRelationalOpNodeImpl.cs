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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
	/// <summary>
	/// Represents a lesser or greater then (&lt;/&lt;=/&gt;/&gt;=) expression in a filter expression tree.
	/// </summary>
	[Serializable]
	public class ExprRelationalOpNodeImpl : ExprNodeBase , ExprRelationalOpNode
	{
	    private readonly RelationalOpEnum relationalOpEnum;

	    [NonSerialized] private ExprRelationalOpNodeForge forge;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="relationalOpEnum">type of compare, ie. lt, gt, le, ge</param>
	    public ExprRelationalOpNodeImpl(RelationalOpEnum relationalOpEnum) {
	        this.relationalOpEnum = relationalOpEnum;
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

	    public bool IsConstantResult
	    {
	        get => false;
	    }

	    /// <summary>
	    /// Returns the type of relational op used.
	    /// </summary>
	    /// <returns>enum with relational op type</returns>
	    public RelationalOpEnum RelationalOpEnum
	    {
	        get => relationalOpEnum;
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext) {
	        // Must have 2 child nodes
	        if (this.ChildNodes.Length != 2) {
	            throw new IllegalStateException("Relational op node does not have exactly 2 parameters");
	        }

	        // Must be either numeric or string
	        Type typeOne = Boxing.GetBoxedType(ChildNodes[0].Forge.EvaluationType);
	        Type typeTwo = Boxing.GetBoxedType(ChildNodes[1].Forge.EvaluationType);

	        if ((typeOne != typeof(string)) || (typeTwo != typeof(string))) {
	            if (!TypeHelper.IsNumeric(typeOne)) {
	                throw new ExprValidationException("Implicit conversion from datatype '" +
	                        (typeOne == null ? "null" : typeOne.GetSimpleName()) +
	                        "' to numeric is not allowed");
	            }
	            if (!TypeHelper.IsNumeric(typeTwo)) {
	                throw new ExprValidationException("Implicit conversion from datatype '" +
	                        (typeTwo == null ? "null" : typeTwo.GetSimpleName()) +
	                        "' to numeric is not allowed");
	            }
	        }

	        Type compareType = TypeHelper.GetCompareToCoercionType(typeOne, typeTwo);
	        RelationalOpEnum.Computer computer = relationalOpEnum.GetComputer(compareType, typeOne, typeTwo);
	        forge = new ExprRelationalOpNodeForge(this, computer);
	        return null;
	    }

	    public override void ToPrecedenceFreeEPL(StringWriter writer) {
	        this.ChildNodes[0].ToEPL(writer, Precedence);
	        writer.Write(relationalOpEnum.ExpressionText);
	        this.ChildNodes[1].ToEPL(writer, Precedence);
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;
	    }

	    public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) {
	        if (!(node is ExprRelationalOpNodeImpl)) {
	            return false;
	        }

	        ExprRelationalOpNodeImpl other = (ExprRelationalOpNodeImpl) node;

	        if (other.relationalOpEnum != this.relationalOpEnum) {
	            return false;
	        }

	        return true;
	    }
	}
} // end of namespace