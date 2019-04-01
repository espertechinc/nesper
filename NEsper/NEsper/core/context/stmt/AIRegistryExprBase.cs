///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;

namespace com.espertech.esper.core.context.stmt
{
    public abstract class AIRegistryExprBase : AIRegistryExpr
    {
        private readonly IDictionary<ExprSubselectNode, AIRegistrySubselect> _subselects;
        private readonly IDictionary<ExprSubselectNode, AIRegistryAggregation> _subselectAggregations;
        private readonly IDictionary<ExprPriorNode, AIRegistryPrior> _priors;
        private readonly IDictionary<ExprPreviousNode, AIRegistryPrevious> _previous;
        private readonly AIRegistryMatchRecognizePrevious _matchRecognizePrevious;
        private readonly IDictionary<ExprTableAccessNode, AIRegistryTableAccess> _tableAccess;

        protected AIRegistryExprBase()
        {
            _subselects = new Dictionary<ExprSubselectNode, AIRegistrySubselect>();
            _subselectAggregations = new Dictionary<ExprSubselectNode, AIRegistryAggregation>();
            _priors = new Dictionary<ExprPriorNode, AIRegistryPrior>();
            _previous = new Dictionary<ExprPreviousNode, AIRegistryPrevious>();
            _matchRecognizePrevious = AllocateAIRegistryMatchRecognizePrevious();
            _tableAccess = new NullableDictionary<ExprTableAccessNode, AIRegistryTableAccess>();
        }
    
        public abstract AIRegistrySubselect AllocateAIRegistrySubselect();
        public abstract AIRegistryPrevious AllocateAIRegistryPrevious();
        public abstract AIRegistryPrior AllocateAIRegistryPrior();
        public abstract AIRegistryAggregation AllocateAIRegistrySubselectAggregation();
        public abstract AIRegistryMatchRecognizePrevious AllocateAIRegistryMatchRecognizePrevious();
        public abstract AIRegistryTableAccess AllocateAIRegistryTableAccess();
    
        public AIRegistrySubselect GetSubselectService(ExprSubselectNode exprSubselectNode)
        {
            return _subselects.Get(exprSubselectNode);
        }
    
        public AIRegistryAggregation GetSubselectAggregationService(ExprSubselectNode exprSubselectNode)
        {
            return _subselectAggregations.Get(exprSubselectNode);
        }
    
        public AIRegistryPrior GetPriorServices(ExprPriorNode key)
        {
            return _priors.Get(key);
        }
    
        public AIRegistryPrevious GetPreviousServices(ExprPreviousNode key)
        {
            return _previous.Get(key);
        }
    
        public AIRegistryMatchRecognizePrevious GetMatchRecognizePrevious()
        {
            return _matchRecognizePrevious;
        }

        public AIRegistryTableAccess GetTableAccessServices(ExprTableAccessNode key)
        {
            return _tableAccess.Get(key);
        }
    
        public AIRegistrySubselect AllocateSubselect(ExprSubselectNode subselectNode)
        {
            AIRegistrySubselect subselect = AllocateAIRegistrySubselect();
            _subselects.Put(subselectNode, subselect);
            return subselect;
        }
    
        public AIRegistryAggregation AllocateSubselectAggregation(ExprSubselectNode subselectNode)
        {
            AIRegistryAggregation subselectAggregation = AllocateAIRegistrySubselectAggregation();
            _subselectAggregations.Put(subselectNode, subselectAggregation);
            return subselectAggregation;
        }
    
        public AIRegistryPrior AllocatePrior(ExprPriorNode key)
        {
            AIRegistryPrior service = AllocateAIRegistryPrior();
            _priors.Put(key, service);
            return service;
        }

        public AIRegistryPrior GetOrAllocatePrior(ExprPriorNode key)
        {
            AIRegistryPrior existing = _priors.Get(key);
            if (existing != null)
            {
                return existing;
            }
            return AllocatePrior(key);
        }

        public AIRegistryPrevious GetOrAllocatePrevious(ExprPreviousNode key)
        {
            AIRegistryPrevious existing = _previous.Get(key);
            if (existing != null)
            {
                return existing;
            }
            return AllocatePrevious(key);
        }

        public AIRegistrySubselect GetOrAllocateSubquery(ExprSubselectNode key)
        {
            AIRegistrySubselect existing = _subselects.Get(key);
            if (existing != null)
            {
                return existing;
            }
            return AllocateSubselect(key);
        }

        public AIRegistryAggregation GetOrAllocateSubselectAggregation(ExprSubselectNode subselectNode)
        {
            AIRegistryAggregation existing = _subselectAggregations.Get(subselectNode);
            if (existing != null)
            {
                return existing;
            }
            return AllocateSubselectAggregation(subselectNode);
        }
    
        public AIRegistryPrevious AllocatePrevious(ExprPreviousNode previousNode)
        {
            AIRegistryPrevious service = AllocateAIRegistryPrevious();
            _previous.Put(previousNode, service);
            return service;
        }

        public AIRegistryTableAccess AllocateTableAccess(ExprTableAccessNode tableNode)
        {
            AIRegistryTableAccess service = AllocateAIRegistryTableAccess();
            _tableAccess.Put(tableNode, service);
            return service;
        }

        public AIRegistryMatchRecognizePrevious AllocateMatchRecognizePrevious()
        {
            return _matchRecognizePrevious;
        }

        public int SubselectAgentInstanceCount
        {
            get
            {
                return _subselects.Sum(entry => entry.Value.AgentInstanceCount);
            }
        }

        public int PreviousAgentInstanceCount
        {
            get
            {
                return _previous.Sum(entry => entry.Value.AgentInstanceCount);
            }
        }

        public int PriorAgentInstanceCount
        {
            get
            {
                return _priors.Sum(entry => entry.Value.AgentInstanceCount);
            }
        }

        public void DeassignService(int agentInstanceId)
        {
            foreach (var entry in _subselects)
                entry.Value.DeassignService(agentInstanceId);
            foreach (var entry in _subselectAggregations)
                entry.Value.DeassignService(agentInstanceId);
            foreach (var entry in _priors)
                entry.Value.DeassignService(agentInstanceId);
            foreach (var entry in _previous)
                entry.Value.DeassignService(agentInstanceId);
            foreach (var entry in _tableAccess)
                entry.Value.DeassignService(agentInstanceId);

            _matchRecognizePrevious.DeassignService(agentInstanceId);
        }
    }
}
