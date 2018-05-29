///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Factory for <seealso cref="ExpressionBatchView"/>.
    /// </summary>
    public class ExpressionBatchViewFactory : ExpressionViewFactoryBase, DataWindowBatchingViewFactory
    {
        private bool _includeTriggeringEvent = true;
    
        public override void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters)
        {
            if (expressionParameters.Count != 1 && expressionParameters.Count != 2)
            {
                var errorMessage = ViewName + " view requires a single expression as a parameter, or an expression and bool flag";
                throw new ViewParameterException(errorMessage);
            }
            ExpiryExpression = expressionParameters[0];
    
            if (expressionParameters.Count > 1)
            {
                var result = ViewFactorySupport.EvaluateAssertNoProperties(ViewName, expressionParameters[1], 1, new ExprEvaluatorContextStatement(viewFactoryContext.StatementContext, false));
                _includeTriggeringEvent = result.AsBoolean();
            }
        }
    
        public override View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var builtinBean = new ObjectArrayEventBean(ExpressionViewOAFieldEnumExtensions.GetPrototypeOA(), BuiltinMapType);
            var viewUpdatedCollection = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            return new ExpressionBatchView(this, viewUpdatedCollection, ExpiryExpressionEvaluator, AggregationServiceFactoryDesc, builtinBean, VariableNames, agentInstanceViewFactoryContext);
        }
    
        public override Object MakePreviousGetter()
        {
            return new RelativeAccessByEventNIndexGetterImpl();
        }

        public bool IsIncludeTriggeringEvent => _includeTriggeringEvent;

        public override string ViewName => "Expression-batch";
    }
}
