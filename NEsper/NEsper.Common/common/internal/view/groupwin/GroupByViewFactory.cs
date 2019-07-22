///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
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
        internal bool addingProperties; // when adding properties to the grouped-views output
        internal ExprEvaluator[] criteriaEvals;
        internal Type[] criteriaTypes;
        internal EventType eventType;
        internal ViewFactory[] groupeds;
        internal bool isReclaimAged;
        internal string[] propertyNames;
        internal long reclaimFrequency;
        internal long reclaimMaxAge;

        public EventType EventType {
            get => eventType;
            set => eventType = value;
        }

        public bool IsReclaimAged {
            get => isReclaimAged;
            set => isReclaimAged = value;
        }

        public long ReclaimMaxAge {
            get => reclaimMaxAge;
            set => reclaimMaxAge = value;
        }

        public long ReclaimFrequency {
            get => reclaimFrequency;
            set => reclaimFrequency = value;
        }

        public ExprEvaluator[] CriteriaEvals {
            get => criteriaEvals;
            set => criteriaEvals = value;
        }

        public string[] PropertyNames {
            get => propertyNames;
            set => propertyNames = value;
        }

        public ViewFactory[] Groupeds {
            get => groupeds;
            set => groupeds = value;
        }

        public bool IsAddingProperties {
            get => addingProperties;
            set => addingProperties = value;
        }

        public Type[] CriteriaTypes {
            get => criteriaTypes;
            set => criteriaTypes = value;
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            if (isReclaimAged) {
                return new GroupByViewReclaimAged(this, agentInstanceViewFactoryContext);
            }

            return new GroupByViewImpl(this, agentInstanceViewFactoryContext);
        }

        public string ViewName => ViewEnum.GROUP_PROPERTY.GetViewName();

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
            if (groupeds == null) {
                throw new IllegalStateException("Grouped views not provided");
            }

            foreach (var grouped in groupeds) {
                grouped.Init(viewFactoryContext, services);
            }
        }
    }
} // end of namespace