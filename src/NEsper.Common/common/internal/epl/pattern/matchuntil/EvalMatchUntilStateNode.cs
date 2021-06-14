///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.pattern.matchuntil
{
    /// <summary>
    /// This class represents the state of a match-until node in the evaluation state tree.
    /// </summary>
    public class EvalMatchUntilStateNode : EvalStateNode,
        Evaluator
    {
        protected internal readonly EvalMatchUntilNode evalMatchUntilNode;
        protected internal MatchedEventMap beginState;
        protected internal readonly List<EventBean>[] matchedEventArrays;
        protected internal EvalStateNode stateMatcher;
        protected internal EvalStateNode stateUntil;
        protected internal int numMatches;
        protected internal int? lowerbounds;
        protected internal int? upperbounds;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalMatchUntilNode">is the factory node associated to the state</param>
        public EvalMatchUntilStateNode(
            Evaluator parentNode,
            EvalMatchUntilNode evalMatchUntilNode)
            : base(parentNode)
        {
            matchedEventArrays = new List<EventBean>[evalMatchUntilNode.FactoryNode.TagsArrayed.Length];
            this.evalMatchUntilNode = evalMatchUntilNode;
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            bool quit = PatternConsumptionUtil.ContainsEvent(matchEvent, beginState);
            if (!quit) {
                foreach (List<EventBean> list in matchedEventArrays) {
                    if (list == null) {
                        continue;
                    }

                    foreach (EventBean @event in list) {
                        if (matchEvent.Contains(@event)) {
                            quit = true;
                            break;
                        }
                    }

                    if (quit) {
                        break;
                    }
                }
            }

            if (quit) {
                Quit();
                AgentInstanceContext agentInstanceContext = evalMatchUntilNode.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.PatternFalse(
                    evalMatchUntilNode.FactoryNode,
                    this,
                    agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
            }
            else {
                stateMatcher?.RemoveMatch(matchEvent);
                stateUntil?.RemoveMatch(matchEvent);
            }
        }

        public override EvalNode FactoryNode {
            get => evalMatchUntilNode;
        }

        public override void Start(MatchedEventMap beginState)
        {
            AgentInstanceContext agentInstanceContext = evalMatchUntilNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternMatchUntilStart(
                evalMatchUntilNode.factoryNode,
                beginState);
            agentInstanceContext.AuditProvider.PatternInstance(
                true,
                evalMatchUntilNode.factoryNode,
                agentInstanceContext);

            this.beginState = beginState;

            EvalNode childMatcher = evalMatchUntilNode.ChildNodeSub;
            stateMatcher = childMatcher.NewState(this);

            if (evalMatchUntilNode.ChildNodeUntil != null) {
                EvalNode childUntil = evalMatchUntilNode.ChildNodeUntil;
                stateUntil = childUntil.NewState(this);
            }

            // start until first, it controls the expression
            // if the same event fires both match and until, the match should not count
            stateUntil?.Start(beginState);

            EvalMatchUntilStateBounds bounds = EvalMatchUntilStateBounds.InitBounds(
                evalMatchUntilNode.FactoryNode,
                beginState,
                evalMatchUntilNode.Context);
            lowerbounds = bounds.Lowerbounds;
            upperbounds = bounds.Upperbounds;

            stateMatcher?.Start(beginState);

            agentInstanceContext.InstrumentationProvider.APatternMatchUntilStart();
        }

        public void EvaluateTrue(
            MatchedEventMap matchEvent,
            EvalStateNode fromNode,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            AgentInstanceContext agentInstanceContext = evalMatchUntilNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternMatchUntilEvaluateTrue(
                evalMatchUntilNode.factoryNode,
                matchEvent,
                fromNode == stateUntil);

            bool isMatcher = false;
            if (fromNode == stateMatcher) {
                // Add the additional tagged events to the list for later posting
                isMatcher = true;
                numMatches++;
                int[] tags = evalMatchUntilNode.FactoryNode.TagsArrayed;
                for (int i = 0; i < tags.Length; i++) {
                    object theEvent = matchEvent.GetMatchingEventAsObject(tags[i]);
                    if (theEvent != null) {
                        if (matchedEventArrays[i] == null) {
                            matchedEventArrays[i] = new List<EventBean>();
                        }

                        if (theEvent is EventBean) {
                            matchedEventArrays[i].Add((EventBean) theEvent);
                        }
                        else {
                            EventBean[] arrayEvents = (EventBean[]) theEvent;
                            matchedEventArrays[i].AddAll(arrayEvents);
                        }
                    }
                }
            }

            if (isQuitted) {
                if (isMatcher) {
                    stateMatcher = null;
                }
                else {
                    stateUntil = null;
                }
            }

            // handle matcher evaluating true
            if (isMatcher) {
                if ((IsTightlyBound) && (numMatches == lowerbounds)) {
                    QuitInternal();
                    MatchedEventMap consolidated = Consolidate(
                        matchEvent,
                        matchedEventArrays,
                        evalMatchUntilNode.FactoryNode.TagsArrayed);
                    agentInstanceContext.AuditProvider.PatternTrue(
                        evalMatchUntilNode.FactoryNode,
                        this,
                        consolidated,
                        true,
                        agentInstanceContext);
                    agentInstanceContext.AuditProvider.PatternInstance(
                        false,
                        evalMatchUntilNode.factoryNode,
                        agentInstanceContext);
                    ParentEvaluator.EvaluateTrue(consolidated, this, true, optionalTriggeringEvent);
                }
                else {
                    // restart or keep started if not bounded, or not upper bounds, or upper bounds not reached
                    bool restart = (!IsBounded) ||
                                   (upperbounds == null) ||
                                   (upperbounds > numMatches);
                    if (stateMatcher == null) {
                        if (restart) {
                            EvalNode childMatcher = evalMatchUntilNode.ChildNodeSub;
                            stateMatcher = childMatcher.NewState(this);
                            stateMatcher.Start(beginState);
                        }
                    }
                    else {
                        if (!restart) {
                            stateMatcher.Quit();
                            stateMatcher = null;
                        }
                    }
                }
            }
            else {
                // handle until-node
                QuitInternal();

                // consolidate multiple matched events into a single event
                MatchedEventMap consolidated = Consolidate(
                    matchEvent,
                    matchedEventArrays,
                    evalMatchUntilNode.FactoryNode.TagsArrayed);

                if ((lowerbounds != null) && (numMatches < lowerbounds)) {
                    agentInstanceContext.AuditProvider.PatternFalse(
                        evalMatchUntilNode.FactoryNode,
                        this,
                        agentInstanceContext);
                    agentInstanceContext.AuditProvider.PatternInstance(
                        false,
                        evalMatchUntilNode.factoryNode,
                        agentInstanceContext);
                    ParentEvaluator.EvaluateFalse(this, true);
                }
                else {
                    agentInstanceContext.AuditProvider.PatternTrue(
                        evalMatchUntilNode.FactoryNode,
                        this,
                        consolidated,
                        true,
                        agentInstanceContext);
                    agentInstanceContext.AuditProvider.PatternInstance(
                        false,
                        evalMatchUntilNode.factoryNode,
                        agentInstanceContext);
                    ParentEvaluator.EvaluateTrue(consolidated, this, true, optionalTriggeringEvent);
                }
            }

            agentInstanceContext.InstrumentationProvider.APatternMatchUntilEvaluateTrue(
                stateMatcher == null && stateUntil == null);
        }

        public static MatchedEventMap Consolidate(
            MatchedEventMap beginState,
            List<EventBean>[] matchedEventList,
            int[] tagsArrayed)
        {
            if (tagsArrayed == null) {
                return beginState;
            }

            for (int i = 0; i < tagsArrayed.Length; i++) {
                if (matchedEventList[i] == null) {
                    continue;
                }

                EventBean[] eventsForTag = matchedEventList[i].ToArray();
                beginState.Add(tagsArrayed[i], eventsForTag);
            }

            return beginState;
        }

        public void EvaluateFalse(
            EvalStateNode fromNode,
            bool restartable)
        {
            AgentInstanceContext agentInstanceContext = evalMatchUntilNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternMatchUntilEvalFalse(
                evalMatchUntilNode.factoryNode,
                fromNode == stateUntil);

            bool isMatcher = fromNode == stateMatcher;

            if (isMatcher) {
                stateMatcher.Quit();
                stateMatcher = null;
            }
            else {
                stateUntil.Quit();
                stateUntil = null;
            }

            agentInstanceContext.AuditProvider.PatternFalse(evalMatchUntilNode.FactoryNode, this, agentInstanceContext);
            agentInstanceContext.AuditProvider.PatternInstance(
                false,
                evalMatchUntilNode.factoryNode,
                agentInstanceContext);
            ParentEvaluator.EvaluateFalse(this, true);
            agentInstanceContext.InstrumentationProvider.APatternMatchUntilEvalFalse();
        }

        public override void Quit()
        {
            if (stateMatcher == null && stateUntil == null) {
                return;
            }

            AgentInstanceContext agentInstanceContext = evalMatchUntilNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternMatchUntilQuit(evalMatchUntilNode.factoryNode);
            agentInstanceContext.AuditProvider.PatternInstance(
                false,
                evalMatchUntilNode.factoryNode,
                agentInstanceContext);

            QuitInternal();

            agentInstanceContext.InstrumentationProvider.APatternMatchUntilQuit();
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitMatchUntil(evalMatchUntilNode.FactoryNode, this, matchedEventArrays, beginState);
            stateMatcher?.Accept(visitor);
            stateUntil?.Accept(visitor);
        }

        public override string ToString()
        {
            return "EvalMatchUntilStateNode";
        }

        public override bool IsNotOperator {
            get => false;
        }

        public override bool IsFilterStateNode {
            get => false;
        }

        public bool IsFilterChildNonQuitting {
            get => true;
        }

        public override bool IsObserverStateNodeNonRestarting {
            get => false;
        }

        private bool IsTightlyBound {
            get { return lowerbounds != null && upperbounds != null && upperbounds.Equals(lowerbounds); }
        }

        private bool IsBounded {
            get { return lowerbounds != null || upperbounds != null; }
        }

        private void QuitInternal()
        {
            if (stateMatcher != null) {
                stateMatcher.Quit();
                stateMatcher = null;
            }

            if (stateUntil != null) {
                stateUntil.Quit();
                stateUntil = null;
            }
        }
    }
} // end of namespace