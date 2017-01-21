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
    /// Output condition for output rate limiting that handles when-then expressions for controlling output.
    /// </summary>
    public class OutputConditionExpressionFactory : OutputConditionFactory
    {
        public OutputConditionExpressionFactory(
            ExprNode whenExpressionNode,
            IList<OnTriggerSetAssignment> assignments,
            StatementContext statementContext,
            ExprNode andWhenTerminatedExpr,
            IList<OnTriggerSetAssignment> afterTerminateAssignments,
            bool isStartConditionOnCreation)
        {
            WhenExpressionNodeEval = whenExpressionNode.ExprEvaluator;
            AndWhenTerminatedExpressionNodeEval = andWhenTerminatedExpr != null ? andWhenTerminatedExpr.ExprEvaluator : null;
            IsStartConditionOnCreation = isStartConditionOnCreation;
    
            // determine if using variables
            var variableVisitor = new ExprNodeVariableVisitor();
            whenExpressionNode.Accept(variableVisitor);
            VariableNames = variableVisitor.VariableNames;
    
            // determine if using properties
            bool containsBuiltinProperties = ContainsBuiltinProperties(whenExpressionNode);
            if (!containsBuiltinProperties && assignments != null) {
                foreach (OnTriggerSetAssignment assignment in assignments) {
                    if (ContainsBuiltinProperties(assignment.Expression)) {
                        containsBuiltinProperties = true;
                    }
                }
            }
            if (!containsBuiltinProperties && AndWhenTerminatedExpressionNodeEval != null) {
                containsBuiltinProperties = ContainsBuiltinProperties(andWhenTerminatedExpr);
            }
            if (!containsBuiltinProperties && afterTerminateAssignments != null) {
                foreach (OnTriggerSetAssignment assignment in afterTerminateAssignments) {
                    if (ContainsBuiltinProperties(assignment.Expression)) {
                        containsBuiltinProperties = true;
                    }
                }
            }
    
            if (containsBuiltinProperties)
            {
                BuiltinPropertiesEventType = GetBuiltInEventType(statementContext.EventAdapterService);
            }
    
            if (assignments != null) {
                VariableReadWritePackage = new VariableReadWritePackage(assignments, statementContext.VariableService, statementContext.EventAdapterService);
            }
            else{
                VariableReadWritePackage = null;
            }
    
            if (afterTerminateAssignments != null) {
                VariableReadWritePackageAfterTerminated = new VariableReadWritePackage(afterTerminateAssignments, statementContext.VariableService, statementContext.EventAdapterService);
            }
            else {
                VariableReadWritePackageAfterTerminated = null;
            }
        }
    
        public OutputCondition Make(AgentInstanceContext agentInstanceContext, OutputCallback outputCallback) {
            return new OutputConditionExpression(outputCallback, agentInstanceContext, this, IsStartConditionOnCreation);
        }

        public ExprEvaluator WhenExpressionNodeEval { get; private set; }

        public ExprEvaluator AndWhenTerminatedExpressionNodeEval { get; private set; }

        public VariableReadWritePackage VariableReadWritePackage { get; private set; }

        public VariableReadWritePackage VariableReadWritePackageAfterTerminated { get; private set; }

        public EventType BuiltinPropertiesEventType { get; private set; }

        public ICollection<string> VariableNames { get; private set; }

        public bool IsStartConditionOnCreation { get; set; }

        /// <summary>Build the event type for built-in properties. </summary>
        /// <param name="eventAdapterService">event adapters</param>
        /// <returns>event type</returns>
        public static EventType GetBuiltInEventType(EventAdapterService eventAdapterService)
        {
            return eventAdapterService.CreateAnonymousObjectArrayType(
                typeof(OutputConditionExpressionFactory).FullName, OutputConditionExpressionTypeUtil.TYPEINFO);
        }
    
        private static bool ContainsBuiltinProperties(ExprNode expr)
        {
            var propertyVisitor = new ExprNodeIdentifierVisitor(false);
            expr.Accept(propertyVisitor);
            return propertyVisitor.ExprProperties.IsNotEmpty();
        }
    }
}
