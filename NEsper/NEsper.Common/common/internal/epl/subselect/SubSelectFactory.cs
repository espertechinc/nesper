///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

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
        }

        public void SetSubqueryNumber(int subqueryNumber)
        {
            this.subqueryNumber = subqueryNumber;
        }

        public ViewableActivator Activator {
            get => activator;
        }

        public void SetActivator(ViewableActivator activator)
        {
            this.activator = activator;
        }

        public SubSelectStrategyFactory StrategyFactory {
            get => strategyFactory;
        }

        public void SetStrategyFactory(SubSelectStrategyFactory strategyFactory)
        {
            this.strategyFactory = strategyFactory;
        }

        public void SetHasAggregation(bool hasAggregation)
        {
            this.hasAggregation = hasAggregation;
        }

        public void SetHasPrior(bool hasPrior)
        {
            this.hasPrior = hasPrior;
        }

        public void SetHasPrevious(bool hasPrevious)
        {
            this.hasPrevious = hasPrevious;
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            strategyFactory.Ready(statementContext, activator.EventType);
        }

        public AIRegistryRequirementSubquery RegistryRequirements {
            get => new AIRegistryRequirementSubquery(
                hasAggregation,
                hasPrior,
                hasPrevious,
                strategyFactory.LookupStrategyDesc);
        }
    }
} // end of namespace