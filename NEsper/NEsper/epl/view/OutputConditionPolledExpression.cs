///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionPolledExpression : OutputConditionPolled
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ExprEvaluator _whenExpressionNode;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly VariableReadWritePackage _variableReadWritePackage;

        private readonly ObjectArrayEventBean _builtinProperties;
        private readonly EventBean[] _eventsPerStream;
    
        // ongoing builtin properties
        private int _totalNewEventsCount;
        private int _totalOldEventsCount;
        private int _totalNewEventsSum;
        private int _totalOldEventsSum;
        private long _lastOutputTimestamp;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="whenExpressionNode">the expression to evaluate, returning true when to output</param>
        /// <param name="assignments">is the optional then-clause variable assignments, or null or empty if none</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <throws><seealso cref="ExprValidationException" /> when validation fails</throws>
        public OutputConditionPolledExpression(ExprNode whenExpressionNode, IList<OnTriggerSetAssignment> assignments, AgentInstanceContext agentInstanceContext)
        {
            _whenExpressionNode = whenExpressionNode.ExprEvaluator;
            _agentInstanceContext = agentInstanceContext;
            _eventsPerStream = new EventBean[1];
    
            // determine if using properties
            var containsBuiltinProperties = false;
            if (ContainsBuiltinProperties(whenExpressionNode))
            {
                containsBuiltinProperties = true;
            }
            else
            {
                if (assignments != null)
                {
                    foreach (OnTriggerSetAssignment assignment in assignments)
                    {
                        if (ContainsBuiltinProperties(assignment.Expression))
                        {
                            containsBuiltinProperties = true;
                        }
                    }
                }
            }
    
            if (containsBuiltinProperties)
            {
                EventType oatype = agentInstanceContext.StatementContext.EventAdapterService.CreateAnonymousObjectArrayType(
                    typeof(OutputConditionPolledExpression).FullName, OutputConditionExpressionTypeUtil.TYPEINFO);
                _builtinProperties = new ObjectArrayEventBean(OutputConditionExpressionTypeUtil.GetOAPrototype(), oatype);
                _lastOutputTimestamp = agentInstanceContext.StatementContext.SchedulingService.Time;
            }
    
            if (assignments != null)
            {
                _variableReadWritePackage = new VariableReadWritePackage(assignments, agentInstanceContext.StatementContext.VariableService, agentInstanceContext.StatementContext.EventAdapterService);
            }
            else
            {
                _variableReadWritePackage = null;
            }
        }
    
        public bool UpdateOutputCondition(int newEventsCount, int oldEventsCount)
        {
            _totalNewEventsCount += newEventsCount;
            _totalOldEventsCount += oldEventsCount;
            _totalNewEventsSum += newEventsCount;
            _totalOldEventsSum += oldEventsCount;
    
            var isOutput = Evaluate();
            if (isOutput)
            {
                ResetBuiltinProperties();
    
                // execute assignments
                if (_variableReadWritePackage != null)
                {
                    if (_builtinProperties != null)
                    {
                        PopulateBuiltinProperties();
                        _eventsPerStream[0] = _builtinProperties;
                    }

                    _variableReadWritePackage.WriteVariables(_agentInstanceContext.StatementContext.VariableService, _eventsPerStream, null, _agentInstanceContext);
                }
            }
            return isOutput;
        }

        private void PopulateBuiltinProperties()
        {
            OutputConditionExpressionTypeUtil.Populate(
                _builtinProperties.Properties,
                _totalNewEventsCount, 
                _totalOldEventsCount,
                _totalNewEventsSum, 
                _totalOldEventsSum, 
                _lastOutputTimestamp);
        }
    
        private bool Evaluate()
        {
            if (_builtinProperties != null)
            {
                PopulateBuiltinProperties();
                _eventsPerStream[0] = _builtinProperties;
            }
    
            var result = false;
            var output = (bool?) _whenExpressionNode.Evaluate(new EvaluateParams(_eventsPerStream, true, _agentInstanceContext));
            if (output ?? false)
            {
                result = true;
            }
    
            return result;
        }
    
        private void ResetBuiltinProperties()
        {
            if (_builtinProperties  != null)
            {
                _totalNewEventsCount = 0;
                _totalOldEventsCount = 0;
                _lastOutputTimestamp = _agentInstanceContext.StatementContext.SchedulingService.Time;
            }
        }
    
        private bool ContainsBuiltinProperties(ExprNode expr)
        {
            var propertyVisitor = new ExprNodeIdentifierVisitor(false);
            expr.Accept(propertyVisitor);
            return !propertyVisitor.ExprProperties.IsEmpty();
        }
    }
}
