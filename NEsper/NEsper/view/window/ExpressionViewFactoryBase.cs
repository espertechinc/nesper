///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.util;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Base factory for expression-based window and batch view.
    /// </summary>
    public abstract class ExpressionViewFactoryBase : DataWindowViewFactory, DataWindowViewWithPrevious
    {
        private EventType _eventType;
        protected ExprNode ExpiryExpression;
        protected ISet<String> VariableNames;
        protected AggregationServiceFactoryDesc AggregationServiceFactoryDesc;
        private EventType _builtinMapType;
    
        public void Attach(EventType parentEventType, StatementContext statementContext, ViewFactory optionalParentFactory, IList<ViewFactory> parentViewFactories)
        {
            _eventType = parentEventType;
    
            // define built-in fields
            var builtinTypeDef = ExpressionViewOAFieldEnumExtensions.AsMapOfTypes(_eventType);
            _builtinMapType = statementContext.EventAdapterService.CreateAnonymousObjectArrayType(
                statementContext.StatementId + "_exprview", builtinTypeDef);

            StreamTypeService streamTypeService = new StreamTypeServiceImpl(new EventType[] { _eventType, _builtinMapType }, new String[2], new bool[2], statementContext.EngineURI, false);
    
            // validate expression
            ExpiryExpression = ViewFactorySupport.ValidateExpr(ViewName, statementContext, ExpiryExpression, streamTypeService, 0);
    
            var summaryVisitor = new ExprNodeSummaryVisitor();
            ExpiryExpression.Accept(summaryVisitor);
            if (summaryVisitor.HasSubselect || summaryVisitor.HasStreamSelect || summaryVisitor.HasPreviousPrior) {
                throw new ViewParameterException("Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context");
            }
    
            var returnType = ExpiryExpression.ExprEvaluator.ReturnType;
            if (returnType.GetBoxedType() != typeof(bool?)) {
                throw new ViewParameterException("Invalid return value for expiry expression, expected a bool return value but received " + returnType.GetParameterAsString());
            }
    
            // determine variables used, if any
            var visitor = new ExprNodeVariableVisitor();
            ExpiryExpression.Accept(visitor);
            VariableNames = visitor.VariableNames;
    
            // determine aggregation nodes, if any
            var aggregateNodes = new List<ExprAggregateNode>();
            ExprAggregateNodeUtil.GetAggregatesBottomUp(ExpiryExpression, aggregateNodes);
            if (aggregateNodes.IsNotEmpty()) {
                try {
                    AggregationServiceFactoryDesc = AggregationServiceFactoryFactory.GetService(
                        Collections.GetEmptyList<ExprAggregateNode>(),
                        Collections.GetEmptyMap<ExprNode, String>(),
                        Collections.GetEmptyList<ExprDeclaredNode>(),
                        null, aggregateNodes, 
                        Collections.GetEmptyList<ExprAggregateNode>(), 
                        Collections.GetEmptyList<ExprAggregateNodeGroupKey>(), false, 
                        statementContext.Annotations, 
                        statementContext.VariableService, false, false, null, null, 
                        statementContext.AggregationServiceFactoryService, 
                        streamTypeService.EventTypes, 
                        statementContext.MethodResolutionService, null,
                        statementContext.ContextName,
                        null, null, false, false, false);
                }
                catch (ExprValidationException ex) {
                    throw new ViewParameterException(ex.Message, ex);
                }
            }
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public bool CanReuse(View view)
        {
            return false;
        }

        public EventType BuiltinMapType
        {
            get { return _builtinMapType; }
        }

        public abstract void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters);
        public abstract View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext);
        public abstract string ViewName { get; }
        public abstract object MakePreviousGetter();
    }
}
