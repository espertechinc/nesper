///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectFactory : StatementReadyCallback
    {
        private int subqueryNumber;
        private ViewableActivator activator;
        private SubSelectStrategyFactory strategyFactory;
        private bool hasAggregation;
        private bool hasPrior;
        private bool hasPrevious;

        public int SubqueryNumber {
            get => subqueryNumber;
            set => subqueryNumber = value;
        }

        public ViewableActivator Activator {
            get => activator;
            set => activator = value;
        }

        public SubSelectStrategyFactory StrategyFactory {
            get => strategyFactory;
            set => strategyFactory = value;
        }

        public bool HasAggregation {
            get => hasAggregation;
            set => hasAggregation = value;
        }

        public bool HasPrior {
            get => hasPrior;
            set => hasPrior = value;
        }

        public bool HasPrevious {
            get => hasPrevious;
            set => hasPrevious = value;
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            strategyFactory.Ready(statementContext, activator.EventType);
        }

        public void Ready(
            SubSelectStrategyFactoryContext subselectFactoryContext,
            bool recovery)
        {
            strategyFactory.Ready(subselectFactoryContext, activator.EventType);
        }

        public AIRegistryRequirementSubquery RegistryRequirements =>
            new AIRegistryRequirementSubquery(
                hasAggregation,
                hasPrior,
                hasPrevious,
                strategyFactory.LookupStrategyDesc);
    }
} // end of namespace