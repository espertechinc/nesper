///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.ops
{
	public class ExprConcatNodeEvalWNew : ExprEvaluator
	{
	    private readonly ExprConcatNode _parent;
	    private readonly ExprEvaluator[] _evaluators;

	    public ExprConcatNodeEvalWNew(ExprConcatNode parent, ExprEvaluator[] evaluators)
        {
	        _parent = parent;
	        _evaluators = evaluators;
	    }

	    public object Evaluate(EvaluateParams evaluateParams)
	    {
            var buffer = new StringBuilder();
            return Evaluate(evaluateParams, buffer, _evaluators, _parent);
        }

        internal static string Evaluate(EvaluateParams evaluateParams, StringBuilder buffer, ExprEvaluator[] evaluators, ExprConcatNode parent)
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprConcat(parent);}
	        foreach (ExprEvaluator child in evaluators)
	        {
	            var resultX = (string) child.Evaluate(evaluateParams);
	            if (resultX == null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprConcat(null);}
	                return null;
	            }
	            buffer.Append(resultX);
	        }
	        var result = buffer.ToString();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprConcat(result);}
	        return result;
	    }

	    public Type ReturnType
	    {
	        get { return typeof (string); }
	    }
	}
} // end of namespace
