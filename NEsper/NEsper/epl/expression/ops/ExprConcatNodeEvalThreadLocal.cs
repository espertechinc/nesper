///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.ops
{
	public class ExprConcatNodeEvalThreadLocal : ExprEvaluator
	{
	    private readonly ExprConcatNode _parent;
	    private readonly ExprEvaluator[] _evaluators;

	    private readonly IThreadLocal<StringBuilder> _localBuffer;

	    public ExprConcatNodeEvalThreadLocal(
	        ExprConcatNode parent,
	        ExprEvaluator[] evaluators,
	        IThreadLocalManager threadLocalManager)
	    {
	        _parent = parent;
	        _evaluators = evaluators;
            _localBuffer = threadLocalManager.Create<StringBuilder>(() => new StringBuilder());
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
	        var buffer = _localBuffer.GetOrCreate();
	        buffer.Length = 0;
	        return ExprConcatNodeEvalWNew.Evaluate(evaluateParams, buffer, _evaluators, _parent);
	    }

	    public Type ReturnType
	    {
	        get { return typeof (string); }
	    }
	}
} // end of namespace
