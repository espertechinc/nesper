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
using com.espertech.esper.events;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    ///     Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionExpressionFactory : OutputConditionFactory
    {
        private readonly ExprEvaluator _andWhenTerminatedExpressionNodeEval;
        private readonly EventType _builtinPropertiesEventType;
        private readonly bool _isStartConditionOnCreation;
        private readonly ISet<string> _variableNames;
        private readonly VariableReadWritePackage _variableReadWritePackage;
        private readonly VariableReadWritePackage _variableReadWritePackageAfterTerminated;
        private readonly ExprEvaluator _whenExpressionNodeEval;

        public OutputConditionExpressionFactory(
            ExprNode whenExpressionNode,
            IList<OnTriggerSetAssignment> assignments,
            StatementContext statementContext,
            ExprNode andWhenTerminatedExpr,
            IList<OnTriggerSetAssignment> afterTerminateAssignments,
            bool isStartConditionOnCreation)
        {
            _whenExpressionNodeEval = whenExpressionNode.ExprEvaluator;
            _andWhenTerminatedExpressionNodeEval = andWhenTerminatedExpr != null ? andWhenTerminatedExpr.ExprEvaluator : null;
            _isStartConditionOnCreation = isStartConditionOnCreation;

            // determine if using variables
            var variableVisitor = new ExprNodeVariableVisitor(statementContext.VariableService);
            whenExpressionNode.Accept(variableVisitor);
            _variableNames = variableVisitor.VariableNames;

            // determine if using properties
            bool containsBuiltinProperties = ContainsBuiltinProperties(whenExpressionNode);
            if (!containsBuiltinProperties && assignments != null)
            {
                foreach (OnTriggerSetAssignment assignment in assignments)
                {
                    if (ContainsBuiltinProperties(assignment.Expression))
                    {
                        containsBuiltinProperties = true;
                    }
                }
            }
            if (!containsBuiltinProperties && _andWhenTerminatedExpressionNodeEval != null)
            {
                containsBuiltinProperties = ContainsBuiltinProperties(andWhenTerminatedExpr);
            }
            if (!containsBuiltinProperties && afterTerminateAssignments != null)
            {
                foreach (OnTriggerSetAssignment assignment in afterTerminateAssignments)
                {
                    if (ContainsBuiltinProperties(assignment.Expression))
                    {
                        containsBuiltinProperties = true;
                    }
                }
            }

            if (containsBuiltinProperties)
            {
                _builtinPropertiesEventType = GetBuiltInEventType(statementContext.EventAdapterService);
            }

            if (assignments != null)
            {
                _variableReadWritePackage = new VariableReadWritePackage(
                    assignments, statementContext.VariableService, statementContext.EventAdapterService);
            }
            else
            {
                _variableReadWritePackage = null;
            }

            if (afterTerminateAssignments != null)
            {
                _variableReadWritePackageAfterTerminated = new VariableReadWritePackage(
                    afterTerminateAssignments, statementContext.VariableService, statementContext.EventAdapterService);
            }
            else
            {
                _variableReadWritePackageAfterTerminated = null;
            }
        }

        public OutputCondition Make(AgentInstanceContext agentInstanceContext, OutputCallback outputCallback)
        {
            return new OutputConditionExpression(outputCallback, agentInstanceContext, this, _isStartConditionOnCreation);
        }

        /// <summary>
        ///     Build the event type for built-in properties.
        /// </summary>
        /// <param name="eventAdapterService">event adapters</param>
        /// <returns>event type</returns>
        public static EventType GetBuiltInEventType(EventAdapterService eventAdapterService)
        {
            return eventAdapterService.CreateAnonymousObjectArrayType(
                typeof (OutputConditionExpressionFactory).FullName, OutputConditionExpressionTypeUtil.TYPEINFO);
        }

        public ExprEvaluator WhenExpressionNodeEval => _whenExpressionNodeEval;

        public ExprEvaluator AndWhenTerminatedExpressionNodeEval => _andWhenTerminatedExpressionNodeEval;

        public VariableReadWritePackage VariableReadWritePackage => _variableReadWritePackage;

        public VariableReadWritePackage VariableReadWritePackageAfterTerminated => _variableReadWritePackageAfterTerminated;

        public EventType BuiltinPropertiesEventType => _builtinPropertiesEventType;

        public ISet<string> VariableNames => _variableNames;

        private bool ContainsBuiltinProperties(ExprNode expr)
        {
            var propertyVisitor = new ExprNodeIdentifierVisitor(false);
            expr.Accept(propertyVisitor);
            return !propertyVisitor.ExprProperties.IsEmpty();
        }
    }
} // end of namespace