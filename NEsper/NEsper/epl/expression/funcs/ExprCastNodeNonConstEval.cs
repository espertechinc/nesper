///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.funcs
{
	public class ExprCastNodeNonConstEval : ExprEvaluator
	{
	    private readonly ExprCastNode _parent;
	    private readonly ExprEvaluator _evaluator;
        private readonly ExprCastNode.ComputeCaster _casterParserComputer;

	    internal ExprCastNodeNonConstEval(ExprCastNode parent, ExprEvaluator evaluator, ExprCastNode.ComputeCaster casterParserComputer)
        {
	        _parent = parent;
	        _evaluator = evaluator;
	        _casterParserComputer = casterParserComputer;
	    }

	    public object Evaluate(EvaluateParams evaluateParams)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprCast(_parent);}

	        var result = _evaluator.Evaluate(evaluateParams);
	        if (result != null) {
	            result = _casterParserComputer.Invoke(result, evaluateParams);
	        }

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprCast(result);}
	        return result;
	    }

	    public Type ReturnType
	    {
	        get { return _parent.TargetType; }
	    }
	}
} // end of namespace
