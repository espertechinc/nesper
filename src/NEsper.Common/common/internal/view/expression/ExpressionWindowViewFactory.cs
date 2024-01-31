///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.expression
{
    public class ExpressionWindowViewFactory : ExpressionViewFactoryBase
    {
        public override string ViewName => ViewEnum.EXPRESSION_WINDOW.GetViewName();

        public override View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var builtinBean = new ObjectArrayEventBean(
                ExpressionViewOAFieldEnumExtensions.GetPrototypeOA(),
                BuiltinMapType);
            var randomAccess =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRandomAccess(agentInstanceViewFactoryContext);
            return new ExpressionWindowView(this, randomAccess, builtinBean, agentInstanceViewFactoryContext);
        }

        public override PreviousGetterStrategy MakePreviousGetter()
        {
            return new RandomAccessByIndexGetter();
        }
    }
} // end of namespace