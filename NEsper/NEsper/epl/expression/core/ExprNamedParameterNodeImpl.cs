///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
	public class ExprNamedParameterNodeImpl 
        : ExprNodeBase 
        , ExprNamedParameterNode
        , ExprEvaluator
    {
	    private readonly string _parameterName;

	    public ExprNamedParameterNodeImpl(string parameterName)
        {
	        _parameterName = parameterName;
	    }

	    public string ParameterName
	    {
	        get { return _parameterName; }
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        writer.Write(_parameterName);
	        writer.Write(":");
	        if (ChildNodes.Count > 1) {
	            writer.Write("(");
	        }
	        ExprNodeUtility.ToExpressionStringParameterList(ChildNodes, writer);
            if (ChildNodes.Count > 1)
            {
	            writer.Write(")");
	        }
	    }

	    public override ExprEvaluator ExprEvaluator
	    {
	        get { return this; }
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get { return ExprPrecedenceEnum.UNARY; }
	    }

	    public override bool IsConstantResult
	    {
	        get { return false; }
	    }

	    public override bool EqualsNode(ExprNode other, bool ignoreStreamPrefix)
        {
	        if (!(other is ExprNamedParameterNode)) {
	            return false;
	        }
	        var otherNamed = (ExprNamedParameterNode) other;
	        return otherNamed.ParameterName.Equals(_parameterName);
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext)
        {
	        return null;
	    }

	    public object Evaluate(EvaluateParams evaluateParams)
	    {
	        return null;
	    }

	    public Type ReturnType
	    {
	        get { return null; }
	    }
    }
} // end of namespace
