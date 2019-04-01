///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.expression
{
    /// <summary>
    ///     Factory for <seealso cref="ExpressionBatchView" />.
    /// </summary>
    public class ExpressionBatchViewFactory : ExpressionViewFactoryBase
    {
        public bool IsIncludeTriggeringEvent { get; private set; } = true;

        public bool IncludeTriggeringEvent {
            set => IsIncludeTriggeringEvent = value;
        }

        public override string ViewName => ViewEnum.EXPRESSION_BATCH_WINDOW.GetName();

        public override View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var builtinBean = new ObjectArrayEventBean(
                ExpressionViewOAFieldEnumExtensions.GetPrototypeOA(), BuiltinMapType);
            ViewUpdatedCollection viewUpdatedCollection =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            return new ExpressionBatchView(this, viewUpdatedCollection, builtinBean, agentInstanceViewFactoryContext);
        }

        public override PreviousGetterStrategy MakePreviousGetter()
        {
            return new RelativeAccessByEventNIndexGetterImpl();
        }
    }
} // end of namespace