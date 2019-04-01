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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
	/// <summary>
	/// Represents the MAX(a,b) and MIN(a,b) functions is an expression tree.
	/// </summary>
	public class ExprMinMaxRowNode : ExprNodeBase {

	    private readonly MinMaxTypeEnum minMaxTypeEnum;

	    [NonSerialized] private ExprMinMaxRowNodeForge forge;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="minMaxTypeEnum">type of compare</param>
	    public ExprMinMaxRowNode(MinMaxTypeEnum minMaxTypeEnum) {
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
	    public MinMaxTypeEnum MinMaxTypeEnum
	    {
	        get => minMaxTypeEnum;
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext) {
	        if (this.ChildNodes.Length < 2) {
	            throw new ExprValidationException("MinMax node must have at least 2 parameters");
	        }

	        foreach (ExprNode child in ChildNodes) {
	            Type childType = child.Forge.EvaluationType;
	            if (!TypeHelper.IsNumeric(childType)) {
	                throw new ExprValidationException("Implicit conversion from datatype '" +
	                        childType.GetSimpleName() +
	                        "' to numeric is not allowed");
	            }
	        }

	        // Determine result type, set up compute function
	        Type childTypeOne = ChildNodes[0].Forge.EvaluationType;
	        Type childTypeTwo = ChildNodes[1].Forge.EvaluationType;
	        Type resultType = TypeHelper.GetArithmaticCoercionType(childTypeOne, childTypeTwo);

	        for (int i = 2; i < this.ChildNodes.Length; i++) {
	            resultType = TypeHelper.GetArithmaticCoercionType(resultType, ChildNodes[i].Forge.EvaluationType);
	        }
	        forge = new ExprMinMaxRowNodeForge(this, resultType);

	        return null;
	    }

	    public bool IsConstantResult
	    {
	        get => false;
	    }

	    public override void ToPrecedenceFreeEPL(StringWriter writer) {
	        writer.Write(minMaxTypeEnum.ExpressionText);
	        writer.Write('(');

	        this.ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
	        writer.Write(',');
	        this.ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);

	        for (int i = 2; i < this.ChildNodes.Length; i++) {
	            writer.Write(',');
	            this.ChildNodes[i].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
	        }

	        writer.Write(')');
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get => ExprPrecedenceEnum.UNARY;
	    }

	    public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix) {
	        if (!(node is ExprMinMaxRowNode)) {
	            return false;
	        }

	        ExprMinMaxRowNode other = (ExprMinMaxRowNode) node;

	        if (other.minMaxTypeEnum != this.minMaxTypeEnum) {
	            return false;
	        }

	        return true;
	    }
	}
} // end of namespace