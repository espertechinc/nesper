///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.view.window
{
    /// <summary>
    /// Factory for <seealso cref="com.espertech.esper.view.window.ExpressionWindowView" />.
    /// </summary>
    public class ExpressionWindowViewFactory : ExpressionViewFactoryBase
    {
        public override void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> expressionParameters) 
        {
            if (expressionParameters.Count != 1) {
                string errorMessage = ViewName + " view requires a single expression as a parameter";
                throw new ViewParameterException(errorMessage);
            }
            ExpiryExpression = expressionParameters[0];
        }
    
        public override View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var builtinBean = new ObjectArrayEventBean(ExpressionViewOAFieldEnumExtensions.GetPrototypeOA(), BuiltinMapType);
            var randomAccess = agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory.GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext); 
            return new ExpressionWindowView(this, randomAccess, ExpiryExpressionEvaluator, AggregationServiceFactoryDesc, builtinBean, VariableNames, agentInstanceViewFactoryContext);
        }
    
        public override Object MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }

        public override string ViewName => "Expression";
    }
}
