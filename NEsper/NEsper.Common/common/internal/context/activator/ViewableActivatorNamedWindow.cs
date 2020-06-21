///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorNamedWindow : ViewableActivator,
        StatementReadyCallback
    {
        internal ExprEvaluator filterEvaluator;
        internal QueryGraph filterQueryGraph;
        internal NamedWindow namedWindow;
        internal int namedWindowConsumerId;
        internal PropertyEvaluator optPropertyEvaluator;
        internal bool subquery;

        public QueryGraph FilterQueryGraph {
            get => filterQueryGraph;
            set => filterQueryGraph = value;
        }

        public string NamedWindowContextName => namedWindow.StatementContext.ContextName;

        public int NamedWindowConsumerId {
            get => namedWindowConsumerId;
            set => namedWindowConsumerId = value;
        }

        public NamedWindow NamedWindow {
            get => namedWindow;
            set => namedWindow = value;
        }

        public ExprEvaluator FilterEvaluator {
            get => filterEvaluator;
            set => filterEvaluator = value;
        }

        public bool Subquery {
            get => subquery;
            set => subquery = value;
        }

        public PropertyEvaluator OptPropertyEvaluator {
            get => optPropertyEvaluator;
            set => optPropertyEvaluator = value;
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            var namedWindowName = namedWindow.Name;
            var namedWindowDeploymentId = namedWindow.StatementContext.DeploymentId;

            statementContext.NamedWindowConsumerManagementService.AddConsumer(
                namedWindowDeploymentId,
                namedWindowName,
                namedWindowConsumerId,
                statementContext,
                subquery);

            statementContext.AddFinalizeCallback(
                new ProxyStatementFinalizeCallback {
                    ProcStatementDestroyed = context => {
                        statementContext.NamedWindowConsumerManagementService.DestroyConsumer(
                            namedWindowDeploymentId,
                            namedWindowName,
                            context);
                    }
                });
        }

        public EventType EventType => namedWindow.RootView.EventType;

        public ViewableActivationResult Activate(
            AgentInstanceContext agentInstanceContext,
            bool isSubselect,
            bool isRecoveringResilient)
        {
            var nw = (NamedWindowWDirectConsume) namedWindow;
            var consumerDesc = new NamedWindowConsumerDesc(
                namedWindowConsumerId,
                filterEvaluator,
                optPropertyEvaluator,
                agentInstanceContext);
            var consumerView = nw.AddConsumer(consumerDesc, isSubselect);
            return new ViewableActivationResult(consumerView, consumerView, null, false, false, null, null, null);
        }
    }
} // end of namespace