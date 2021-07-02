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
        private int _subqueryNumber;
        private ViewableActivator _activator;
        private SubSelectStrategyFactory _strategyFactory;
        private bool _hasAggregation;
        private bool _hasPrior;
        private bool _hasPrevious;

        public int SubqueryNumber {
            get => _subqueryNumber;
            set => _subqueryNumber = value;
        }

        public ViewableActivator Activator {
            get => _activator;
            set => _activator = value;
        }

        public SubSelectStrategyFactory StrategyFactory {
            get => _strategyFactory;
            set => _strategyFactory = value;
        }

        public bool HasAggregation {
            get => _hasAggregation;
            set => _hasAggregation = value;
        }

        public bool HasPrior {
            get => _hasPrior;
            set => _hasPrior = value;
        }

        public bool HasPrevious {
            get => _hasPrevious;
            set => _hasPrevious = value;
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            _strategyFactory.Ready(statementContext, _activator.EventType);
        }

        public void Ready(
            SubSelectStrategyFactoryContext subselectFactoryContext,
            bool recovery)
        {
            _strategyFactory.Ready(subselectFactoryContext, _activator.EventType);
        }

        public AIRegistryRequirementSubquery RegistryRequirements {
            get => new AIRegistryRequirementSubquery(
                _hasAggregation,
                _hasPrior,
                _hasPrevious,
                _strategyFactory.LookupStrategyDesc);
        }
    }
} // end of namespace