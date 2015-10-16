///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;
using com.espertech.esper.pattern.pool;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents the state of a followed-by operator in the evaluation state tree, 
    /// with a maximum number of instances provided, and with the additional capability to 
    /// engine-wide report on pattern instances.
    /// </summary>
    public class EvalFollowedByWithMaxStateNodeManaged : EvalStateNode, Evaluator
    {
        protected readonly EvalFollowedByNode EvalFollowedByNode;
        protected readonly Dictionary<EvalStateNode, int> Nodes;
        protected readonly int[] CountActivePerChild;
    
        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalFollowedByNode">is the factory node associated to the state</param>
        public EvalFollowedByWithMaxStateNodeManaged(Evaluator parentNode, EvalFollowedByNode evalFollowedByNode)
            : base(parentNode)
        {
            EvalFollowedByNode = evalFollowedByNode;
            Nodes = new Dictionary<EvalStateNode, int>();
            CountActivePerChild = evalFollowedByNode.IsTrackWithMax ? new int[evalFollowedByNode.ChildNodes.Length - 1] : null;
        }
    
        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            PatternConsumptionUtil.ChildNodeRemoveMatches(matchEvent, Nodes.Keys);
        }

        public override EvalNode FactoryNode
        {
            get { return EvalFollowedByNode; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            EvalNode child = EvalFollowedByNode.ChildNodes[0];
            EvalStateNode childState = child.NewState(this, null, 0L);
            Nodes.Put(childState, 0);
            childState.Start(beginState);
        }
    
        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            int index;
            var hasIndex = Nodes.TryGetValue(fromNode, out index);
    
            if (isQuitted)
            {
                Nodes.Remove(fromNode);
                if (hasIndex && index > 0) {
                    if (EvalFollowedByNode.IsTrackWithMax) {
                        CountActivePerChild[index - 1]--;
                    }
                    if (EvalFollowedByNode.IsTrackWithPool) {
                        PatternSubexpressionPoolStmtSvc poolSvc = EvalFollowedByNode.Context.StatementContext.PatternSubexpressionPoolSvc;
                        poolSvc.EngineSvc.DecreaseCount(EvalFollowedByNode, EvalFollowedByNode.Context.AgentInstanceContext);
                        poolSvc.StmtHandler.DecreaseCount();
                    }
                }
            }
    
            // the node may already have quit as a result of an outer state quitting this state,
            // however the callback may still be received; It is fine to ignore this callback. 
            if (!hasIndex)
            {
                return;
            }
    
            // If the match came from the very last filter, need to escalate
            int numChildNodes = EvalFollowedByNode.ChildNodes.Length;
            if (index == (numChildNodes - 1))
            {
                bool isFollowedByQuitted = Nodes.IsEmpty();
                ParentEvaluator.EvaluateTrue(matchEvent, this, isFollowedByQuitted);
            }
            // Else start a new sub-expression for the next-in-line filter
            else
            {
                if (EvalFollowedByNode.IsTrackWithMax) {
                    int max = EvalFollowedByNode.FactoryNode.GetMax(index);
                    if ((max != -1) && (max >=0)) {
                        if (CountActivePerChild[index] >= max) {
                            EvalFollowedByNode.Context.AgentInstanceContext.StatementContext.ExceptionHandlingService.HandleCondition(new ConditionPatternSubexpressionMax(max), EvalFollowedByNode.Context.AgentInstanceContext.StatementContext.EpStatementHandle);
                            return;
                        }
                    }
                }
    
                if (EvalFollowedByNode.IsTrackWithPool) {
                    PatternSubexpressionPoolStmtSvc poolSvc = EvalFollowedByNode.Context.StatementContext.PatternSubexpressionPoolSvc;
                    bool allow = poolSvc.EngineSvc.TryIncreaseCount(EvalFollowedByNode, EvalFollowedByNode.Context.AgentInstanceContext);
                    if (!allow) {
                        return;
                    }
                    poolSvc.StmtHandler.IncreaseCount();
                }
    
                if (EvalFollowedByNode.IsTrackWithMax) {
                    CountActivePerChild[index]++;
                }
    
                EvalNode child = EvalFollowedByNode.ChildNodes[index + 1];
                EvalStateNode childState = child.NewState(this, null, 0L);
                Nodes.Put(childState, index + 1);
                childState.Start(matchEvent);
            }
        }
    
        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            int index;
            
            fromNode.Quit();

            var hasIndex = Nodes.TryGetValue(fromNode, out index);
            Nodes.Remove(fromNode);

            if (hasIndex && index > 0) {
                if (EvalFollowedByNode.IsTrackWithMax) {
                    CountActivePerChild[index - 1]--;
                }
                if (EvalFollowedByNode.IsTrackWithPool) {
                    PatternSubexpressionPoolStmtSvc poolSvc = EvalFollowedByNode.Context.StatementContext.PatternSubexpressionPoolSvc;
                    poolSvc.EngineSvc.DecreaseCount(EvalFollowedByNode, EvalFollowedByNode.Context.AgentInstanceContext);
                    poolSvc.StmtHandler.DecreaseCount();
                }
            }
    
            if (Nodes.IsEmpty())
            {
                ParentEvaluator.EvaluateFalse(this, true);
                Quit();
            }
        }

        public override bool IsNotOperator
        {
            get { return false; }
        }

        public override bool IsFilterStateNode
        {
            get { return false; }
        }

        public bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public override bool IsObserverStateNodeNonRestarting
        {
            get { return false; }
        }

        public override void Quit()
        {
            foreach (var entry in Nodes)
            {
                entry.Key.Quit();
                if (EvalFollowedByNode.IsTrackWithPool) {
                    if (entry.Value > 0) {
                        PatternSubexpressionPoolStmtSvc poolSvc = EvalFollowedByNode.Context.StatementContext.PatternSubexpressionPoolSvc;
                        poolSvc.EngineSvc.DecreaseCount(EvalFollowedByNode, EvalFollowedByNode.Context.AgentInstanceContext);
                        poolSvc.StmtHandler.DecreaseCount();
                    }
                }
            }
        }
    
        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitFollowedBy(EvalFollowedByNode.FactoryNode, this, Nodes, CountActivePerChild);
            foreach (EvalStateNode node in Nodes.Keys) {
                node.Accept(visitor);
            }
        }
    
        public override String ToString()
        {
            return "EvalFollowedByStateNode nodes=" + Nodes.Count;
        }
    }
}
