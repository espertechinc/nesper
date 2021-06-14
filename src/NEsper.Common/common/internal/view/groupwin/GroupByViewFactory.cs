///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.view.groupwin
{
    /// <summary>
    ///     Factory for <seealso cref="GroupByView" /> instances.
    /// </summary>
    public class GroupByViewFactory : ViewFactory
    {
        public EventType EventType { get; set; }

        public bool IsReclaimAged { get; set; }

        public long ReclaimMaxAge { get; set; }

        public long ReclaimFrequency { get; set; }

        public ExprEvaluator CriteriaEval { get; set; }

        public string[] PropertyNames { get; set; }

        public ViewFactory[] Groupeds { get; set; }

        public bool IsAddingProperties { get; set; }

        public Type[] CriteriaTypes { get; set; }

        public DataInputOutputSerde KeySerde { get; set; }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            if (IsReclaimAged) {
                return new GroupByViewReclaimAged(this, agentInstanceViewFactoryContext);
            }

            return new GroupByViewImpl(this, agentInstanceViewFactoryContext);
        }

        public string ViewName => ViewEnum.GROUP_PROPERTY.GetViewName();

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            if (Groupeds == null) {
                throw new IllegalStateException("Grouped views not provided");
            }

            foreach (var grouped in Groupeds) {
                grouped.Init(viewFactoryContext, services);
            }
        }
    }
} // end of namespace