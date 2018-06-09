///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.view
{
	/// <summary>
	/// Output condition for output rate limiting that handles when-then expressions for controlling output.
	/// </summary>
	public class OutputConditionPolledExpressionFactory : OutputConditionPolledFactory
	{
	    private readonly ExprEvaluator _whenExpressionNode;
	    private readonly VariableReadWritePackage _variableReadWritePackage;
	    private readonly EventType _oatypeBuiltinProperties;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="whenExpressionNode">the expression to evaluate, returning true when to output</param>
        /// <param name="assignments">is the optional then-clause variable assignments, or null or empty if none</param>
        /// <param name="statementContext">The statement context.</param>
        /// <throws>ExprValidationException when validation fails</throws>
        public OutputConditionPolledExpressionFactory(
            ExprNode whenExpressionNode, 
            IList<OnTriggerSetAssignment> assignments, 
            StatementContext statementContext)
	    {
	        _whenExpressionNode = whenExpressionNode.ExprEvaluator;

	        // determine if using properties
	        var containsBuiltinProperties = false;
	        if (ContainsBuiltinProperties(whenExpressionNode)) {
	            containsBuiltinProperties = true;
	        }
	        else {
	            if (assignments != null) {
	                foreach (var assignment in assignments) {
	                    if (ContainsBuiltinProperties(assignment.Expression)) {
	                        containsBuiltinProperties = true;
	                    }
	                }
	            }
	        }

	        if (containsBuiltinProperties) {
	            _oatypeBuiltinProperties = statementContext.EventAdapterService.CreateAnonymousObjectArrayType(typeof(OutputConditionPolledExpressionFactory).Name, OutputConditionExpressionTypeUtil.TYPEINFO);
	        }
	        else {
	            _oatypeBuiltinProperties = null;
	        }

	        if (assignments != null) {
	            _variableReadWritePackage = new VariableReadWritePackage(assignments, statementContext.VariableService, statementContext.EventAdapterService);
	        }
	        else {
	            _variableReadWritePackage = null;
	        }
	    }

	    public OutputConditionPolled MakeFromState(AgentInstanceContext agentInstanceContext, OutputConditionPolledState state) {
	        ObjectArrayEventBean builtinProperties = null;
	        if (_oatypeBuiltinProperties != null) {
	            builtinProperties = new ObjectArrayEventBean(OutputConditionExpressionTypeUtil.OAPrototype, _oatypeBuiltinProperties);
	        }
	        var expressionState = (OutputConditionPolledExpressionState) state;
	        return new OutputConditionPolledExpression(this, expressionState, agentInstanceContext, builtinProperties);
	    }

	    public OutputConditionPolled MakeNew(AgentInstanceContext agentInstanceContext) {
	        ObjectArrayEventBean builtinProperties = null;
	        long? lastOutputTimestamp = null;
	        if (_oatypeBuiltinProperties != null) {
	            builtinProperties = new ObjectArrayEventBean(OutputConditionExpressionTypeUtil.OAPrototype, _oatypeBuiltinProperties);
	            lastOutputTimestamp = agentInstanceContext.StatementContext.SchedulingService.Time;
	        }
	        var state = new OutputConditionPolledExpressionState(0, 0, 0, 0, lastOutputTimestamp);
	        return new OutputConditionPolledExpression(this, state, agentInstanceContext, builtinProperties);
	    }

	    public ExprEvaluator WhenExpressionNode => _whenExpressionNode;

	    public VariableReadWritePackage VariableReadWritePackage => _variableReadWritePackage;

	    private bool ContainsBuiltinProperties(ExprNode expr)
	    {
	        var propertyVisitor = new ExprNodeIdentifierVisitor(false);
	        expr.Accept(propertyVisitor);
	        return !propertyVisitor.ExprProperties.IsEmpty();
	    }
	}
} // end of namespace
