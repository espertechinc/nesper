///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.pattern.and
{
    /// <summary>
    ///     This class represents the state of an "and" operator in the evaluation state tree.
    /// </summary>
    public class EvalAndStateNode : EvalStateNode,
        Evaluator
    {
        internal readonly EvalStateNode[] activeChildNodes;
        internal readonly EvalAndNode evalAndNode;
        internal object[] eventsPerChild;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalAndNode">is the factory node associated to the state</param>
        public EvalAndStateNode(
            Evaluator parentNode,
            EvalAndNode evalAndNode)
            : base(parentNode)
        {
            this.evalAndNode = evalAndNode;
            activeChildNodes = new EvalStateNode[evalAndNode.ChildNodes.Length];
            eventsPerChild = new object[evalAndNode.ChildNodes.Length];
        }

        public override EvalNode FactoryNode => evalAndNode;

        public override bool IsFilterStateNode => false;

        public override bool IsNotOperator => false;

        public override bool IsObserverStateNodeNonRestarting => false;

        public bool IsFilterChildNonQuitting => false;

        public void EvaluateTrue(
            MatchedEventMap matchEvent,
            EvalStateNode fromNode,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            var agentInstanceContext = evalAndNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternAndEvaluateTrue(evalAndNode.factoryNode, matchEvent);

            int? indexFrom = null;
            for (var i = 0; i < activeChildNodes.Length; i++) {
                if (activeChildNodes[i] == fromNode) {
                    indexFrom = i;
                }
            }

            // If one of the children quits, remove the child
            if (isQuitted && indexFrom != null) {
                activeChildNodes[indexFrom.Value] = null;
            }

            if (eventsPerChild == null || indexFrom == null) {
                agentInstanceContext.InstrumentationProvider.APatternAndEvaluateTrue(true);
                return;
            }

            // If all nodes have events received, the AND expression turns true
            var allHaveEventsExcludingFromChild = true;
            for (var i = 0; i < eventsPerChild.Length; i++) {
                if (indexFrom != i && eventsPerChild[i] == null) {
                    allHaveEventsExcludingFromChild = false;
                    break;
                }
            }

            // if we don't have events from all child nodes, add event and done
            if (!allHaveEventsExcludingFromChild) {
                AddMatchEvent(eventsPerChild, indexFrom.Value, matchEvent);
                agentInstanceContext.InstrumentationProvider.APatternAndEvaluateTrue(false);
                return;
            }

            // if all other nodes have quit other then the from-node, don't retain matching event
            var allOtherNodesQuit = true;
            var hasActive = false;
            for (var i = 0; i < eventsPerChild.Length; i++) {
                if (activeChildNodes[i] != null) {
                    hasActive = true;
                    if (i != indexFrom) {
                        allOtherNodesQuit = false;
                    }
                }
            }

            // if not all other nodes have quit, add event to received list
            if (!allOtherNodesQuit) {
                AddMatchEvent(eventsPerChild, indexFrom.Value, matchEvent);
            }

            // For each combination in eventsPerChild for all other state nodes generate an event to the parent
            var result = GenerateMatchEvents(matchEvent, eventsPerChild, indexFrom.Value);

            // Check if this is quitting
            var quitted = true;
            if (hasActive) {
                foreach (var stateNode in activeChildNodes) {
                    if (stateNode != null && !stateNode.IsNotOperator) {
                        quitted = false;
                    }
                }
            }

            // So we are quitting if all non-not child nodes have quit, since the not-node wait for evaluate false
            if (quitted) {
                agentInstanceContext.AuditProvider.PatternInstance(
                    false,
                    evalAndNode.factoryNode,
                    agentInstanceContext);
                QuitInternal();
            }

            // Send results to parent
            foreach (var theEvent in result) {
                agentInstanceContext.AuditProvider.PatternTrue(
                    evalAndNode.FactoryNode,
                    this,
                    theEvent,
                    quitted,
                    agentInstanceContext);
                ParentEvaluator.EvaluateTrue(theEvent, this, quitted, optionalTriggeringEvent);
            }

            agentInstanceContext.InstrumentationProvider.APatternAndEvaluateTrue(quitted);
        }

        public void EvaluateFalse(
            EvalStateNode fromNode,
            bool restartable)
        {
            var agentInstanceContext = evalAndNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternAndEvaluateFalse(evalAndNode.factoryNode);

            int? indexFrom = null;
            for (var i = 0; i < activeChildNodes.Length; i++) {
                if (activeChildNodes[i] == fromNode) {
                    activeChildNodes[i] = null;
                    indexFrom = i;
                }
            }

            if (indexFrom != null) {
                eventsPerChild[indexFrom.Value] = null;
            }

            // The and node cannot turn true anymore, might as well quit all child nodes
            QuitInternal();

            agentInstanceContext.AuditProvider.PatternFalse(evalAndNode.FactoryNode, this, agentInstanceContext);
            agentInstanceContext.AuditProvider.PatternInstance(false, evalAndNode.factoryNode, agentInstanceContext);
            ParentEvaluator.EvaluateFalse(this, restartable ? true : false);

            agentInstanceContext.InstrumentationProvider.APatternAndEvaluateFalse();
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            var quit = false;
            if (eventsPerChild != null) {
                foreach (var entry in eventsPerChild) {
                    if (entry is MatchedEventMap eventMap) {
                        quit = PatternConsumptionUtil.ContainsEvent(matchEvent, eventMap);
                    }
                    else if (entry != null) {
                        var list = (IList<MatchedEventMap>)entry;
                        foreach (var map in list) {
                            quit = PatternConsumptionUtil.ContainsEvent(matchEvent, map);
                            if (quit) {
                                break;
                            }
                        }
                    }

                    if (quit) {
                        break;
                    }
                }
            }

            if (!quit && activeChildNodes != null) {
                foreach (var child in activeChildNodes) {
                    child?.RemoveMatch(matchEvent);
                }
            }

            if (quit) {
                Quit();
                var agentInstanceContext = evalAndNode.Context.AgentInstanceContext;
                agentInstanceContext.AuditProvider.PatternFalse(evalAndNode.FactoryNode, this, agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
            }
        }

        public override void Start(MatchedEventMap beginState)
        {
            var agentInstanceContext = evalAndNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternAndStart(evalAndNode.factoryNode, beginState);
            agentInstanceContext.AuditProvider.PatternInstance(true, evalAndNode.factoryNode, agentInstanceContext);

            // In an "and" expression we need to create a state for all child listeners
            var count = 0;
            foreach (var node in evalAndNode.ChildNodes) {
                var childState = node.NewState(this);
                activeChildNodes[count++] = childState;
            }

            // Start all child nodes
            foreach (var child in activeChildNodes) {
                child?.Start(beginState);
            }

            agentInstanceContext.InstrumentationProvider.APatternAndStart();
        }

        /// <summary>
        ///     Generate a list of matching event combinations consisting of the events per child that are passed in.
        /// </summary>
        /// <param name="matchEvent">can be populated with prior events that must be passed on</param>
        /// <param name="eventsPerChild">is the list of events for each child node to the "And" node.</param>
        /// <param name="indexFrom">from-index</param>
        /// <returns>list of events populated with all possible combinations</returns>
        public static IList<MatchedEventMap> GenerateMatchEvents(
            MatchedEventMap matchEvent,
            object[] eventsPerChild,
            int indexFrom)
        {
            // Place event list for each child state node into an array, excluding the node where the event came from
            var listArray = new List<IList<MatchedEventMap>>();
            var index = 0;
            for (var i = 0; i < eventsPerChild.Length; i++) {
                var eventsChild = eventsPerChild[i];
                if (indexFrom != i && eventsChild != null) {
                    if (eventsChild is MatchedEventMap map) {
                        listArray.Insert(index++, Collections.SingletonList(map));
                    }
                    else {
                        listArray.Insert(index++, (IList<MatchedEventMap>)eventsChild);
                    }
                }
            }

            // Recursively generate MatchedEventMap instances for all accumulated events
            IList<MatchedEventMap> results = new List<MatchedEventMap>();
            GenerateMatchEvents(listArray, 0, results, matchEvent);

            return results;
        }

        /// <summary>
        ///     For each combination of MatchedEventMap instance in all collections, add an entry to the list.
        ///     Recursive method.
        /// </summary>
        /// <param name="eventList">is an array of lists containing MatchedEventMap instances to combine</param>
        /// <param name="index">is the current index into the array</param>
        /// <param name="result">is the resulting list of MatchedEventMap</param>
        /// <param name="matchEvent">is the start MatchedEventMap to generate from</param>
        protected internal static void GenerateMatchEvents(
            IList<IList<MatchedEventMap>> eventList,
            int index,
            IList<MatchedEventMap> result,
            MatchedEventMap matchEvent)
        {
            var events = eventList[index];

            foreach (var theEvent in events) {
                var current = matchEvent.ShallowCopy();
                current.Merge(theEvent);

                // If this is the very last list in the array of lists, add accumulated MatchedEventMap events to result
                if (index + 1 == eventList.Count) {
                    result.Add(current);
                }
                else {
                    // make a copy of the event collection and hand to next list of events
                    GenerateMatchEvents(eventList, index + 1, result, current);
                }
            }
        }

        public override void Quit()
        {
            if (eventsPerChild == null) {
                return;
            }

            var agentInstanceContext = evalAndNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternAndQuit(evalAndNode.factoryNode);
            agentInstanceContext.AuditProvider.PatternInstance(false, evalAndNode.factoryNode, agentInstanceContext);
            QuitInternal();
            agentInstanceContext.InstrumentationProvider.APatternAndQuit();
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitAnd(evalAndNode.FactoryNode, this, eventsPerChild);
            foreach (var node in activeChildNodes) {
                node?.Accept(visitor);
            }
        }

        public override string ToString()
        {
            return "EvalAndStateNode";
        }

        public static void AddMatchEvent(
            object[] eventsPerChild,
            int indexFrom,
            MatchedEventMap matchEvent)
        {
            var matchEventHolder = eventsPerChild[indexFrom];
            if (matchEventHolder == null) {
                eventsPerChild[indexFrom] = matchEvent;
            }
            else if (matchEventHolder is MatchedEventMap map) {
                IList<MatchedEventMap> list = new List<MatchedEventMap>(4);
                list.Add(map);
                list.Add(matchEvent);
                eventsPerChild[indexFrom] = list;
            }
            else {
                var list = (IList<MatchedEventMap>)matchEventHolder;
                list.Add(matchEvent);
            }
        }

        private void QuitInternal()
        {
            foreach (var child in activeChildNodes) {
                child?.Quit();
            }

            activeChildNodes.Fill((EvalStateNode)null);
            eventsPerChild = null;
        }
    }
} // end of namespace