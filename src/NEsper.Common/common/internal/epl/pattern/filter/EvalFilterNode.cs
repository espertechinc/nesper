///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.pattern.filter
{
    /// <summary>
    ///     This class represents a filter of events in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalFilterNode : EvalNodeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal readonly EvalFilterFactoryNode factoryNode;

        public EvalFilterNode(
            PatternAgentInstanceContext context,
            EvalFilterFactoryNode factoryNode)
            : base(context)
        {
            this.factoryNode = factoryNode;

            FilterValueSetParam[][] addendum = null;
            if (context.AgentInstanceContext.AgentInstanceFilterProxy != null) {
                addendum = context.AgentInstanceContext.AgentInstanceFilterProxy.GetAddendumFilters(
                    factoryNode.FilterSpec,
                    context.AgentInstanceContext);
            }

            var contextPathAddendum = context.GetFilterAddendumForContextPath(factoryNode.FilterSpec);
            if (contextPathAddendum != null) {
                if (addendum == null) {
                    addendum = contextPathAddendum;
                }
                else {
                    addendum = FilterAddendumUtil.MultiplyAddendum(addendum, contextPathAddendum);
                }
            }

            AddendumFilters = addendum;
        }

        public EvalFilterFactoryNode FactoryNode => factoryNode;

        public FilterValueSetParam[][] AddendumFilters { get; }

        public override EvalStateNode NewState(Evaluator parentNode)
        {
            if (Context.ConsumptionHandler != null) {
                return new EvalFilterStateNodeConsumeImpl(parentNode, this);
            }

            return new EvalFilterStateNode(parentNode, this);
        }
    }
} // end of namespace