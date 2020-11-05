///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.view.lengthbatch
{
    public class LengthBatchViewFactory : DataWindowViewFactory,
        DataWindowViewWithPrevious
    {
        protected internal EventType eventType;

        /// <summary>
        ///     The length window size.
        /// </summary>
        protected internal ExprEvaluator size;

        public ExprEvaluator SizeEvaluator {
            get => size;
            set => size = value;
        }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            var size = ViewFactoryUtil.EvaluateSizeParam(
                ViewName,
                this.size,
                agentInstanceViewFactoryContext.AgentInstanceContext);
            var viewUpdatedCollection =
                agentInstanceViewFactoryContext.StatementContext.ViewServicePreviousFactory
                    .GetOptPreviousExprRelativeAccess(agentInstanceViewFactoryContext);
            if (agentInstanceViewFactoryContext.IsRemoveStream) {
                return new LengthBatchViewRStream(agentInstanceViewFactoryContext, this, size);
            }

            return new LengthBatchView(agentInstanceViewFactoryContext, this, size, viewUpdatedCollection);
        }

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public string ViewName => ViewEnum.LENGTH_BATCH.GetViewName();

        public PreviousGetterStrategy MakePreviousGetter()
        {
            return new RelativeAccessByEventNIndexGetterImpl();
        }
    }
} // end of namespace