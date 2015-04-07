///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
	public class ExprCastNodeConstEval : ExprEvaluator
	{
	    private readonly ExprCastNode _parent;
	    private readonly object _theConstant;

	    public ExprCastNodeConstEval(ExprCastNode parent, object theConstant) {
	        _parent = parent;
	        _theConstant = theConstant;
	    }

	    public Type ReturnType
	    {
	        get { return _parent.TargetType; }
	    }

        public object Evaluate(EvaluateParams evaluateParams)
        {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.Get().QExprCast(_parent);
	            InstrumentationHelper.Get().AExprCast(_theConstant);
	        }
	        return _theConstant;
	    }
	}
} // end of namespace
