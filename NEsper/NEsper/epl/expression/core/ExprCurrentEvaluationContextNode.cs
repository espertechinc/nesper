///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.core
{
	/// <summary>
	/// Represents the "current_evaluation_context" function in an expression tree.
	/// </summary>
	[Serializable]
    public class ExprCurrentEvaluationContextNode 
        : ExprNodeBase
        , ExprEvaluator
	{
	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public ExprCurrentEvaluationContextNode()
	    {
	    }

	    public override ExprEvaluator ExprEvaluator
	    {
	        get { return this; }
	    }

	    public override ExprNode Validate(ExprValidationContext validationContext)
	    {
	        if (this.ChildNodes.Count != 0)
	        {
	            throw new ExprValidationException("current_evaluation_context function node cannot have a child node");
	        }
	        return null;
	    }

	    public override bool IsConstantResult
	    {
	        get { return false; }
	    }

	    public Type ReturnType
	    {
	        get { return typeof (EPLExpressionEvaluationContext); }
	    }

	    public object Evaluate(EvaluateParams evaluateParams)
	    {
	        var exprEvaluatorContext = evaluateParams.ExprEvaluatorContext;
	        var ctx = new EPLExpressionEvaluationContext(exprEvaluatorContext.StatementName, exprEvaluatorContext.AgentInstanceId, exprEvaluatorContext.EngineURI, exprEvaluatorContext.StatementUserObject);
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QaExprConst(ctx);}
	        return ctx;
	    }

	    public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
	        writer.Write("current_evaluation_context()");
	    }

	    public override ExprPrecedenceEnum Precedence
	    {
	        get { return ExprPrecedenceEnum.UNARY; }
	    }

	    public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
	    {
	        return node is ExprCurrentEvaluationContextNode;
	    }
	}
} // end of namespace
