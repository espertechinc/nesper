///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents the state of an "and" operator in the evaluation state tree.
    /// </summary>
    public class EvalAndStateNode : EvalStateNode, Evaluator
    {
        private readonly EvalAndNode _evalAndNode;
        private readonly IList<EvalStateNode> _activeChildNodes;
        private Object[] _eventsPerChild;

        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalAndNode">is the factory node associated to the state</param>
        public EvalAndStateNode(Evaluator parentNode, EvalAndNode evalAndNode)
            : base(parentNode)
        {
            _evalAndNode = evalAndNode;
            _activeChildNodes = new EvalStateNode[evalAndNode.ChildNodes.Count];
            _eventsPerChild = new Object[evalAndNode.ChildNodes.Count];
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            bool quit = false;
            if (_eventsPerChild != null)
            {
                foreach (Object entry in _eventsPerChild)
                {
                    if (entry is MatchedEventMap)
                    {
                        quit = PatternConsumptionUtil.ContainsEvent(matchEvent, (MatchedEventMap)entry);
                    }
                    else if (entry != null)
                    {
                        var list = (IList<MatchedEventMap>)entry;
                        foreach (MatchedEventMap map in list)
                        {
                            quit = PatternConsumptionUtil.ContainsEvent(matchEvent, map);
                            if (quit)
                            {
                                break;
                            }
                        }
                    }
                    if (quit)
                    {
                        break;
                    }
                }
            }
            if (!quit && _activeChildNodes != null)
            {
                foreach (EvalStateNode child in _activeChildNodes)
                {
                    if (child != null)
                    {
                        child.RemoveMatch(matchEvent);
                    }
                }
            }
            if (quit)
            {
                Quit();
                ParentEvaluator.EvaluateFalse(this, true);
            }
        }

        public override EvalNode FactoryNode
        {
            get { return _evalAndNode; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternAndStart(_evalAndNode, beginState); }
            // In an "and" expression we need to create a state for all child listeners
            int count = 0;
            foreach (EvalNode node in _evalAndNode.ChildNodes)
            {
                EvalStateNode childState = node.NewState(this, null, 0L);
                _activeChildNodes[count++] = childState;
            }

            // Start all child nodes
            foreach (EvalStateNode child in _activeChildNodes)
            {
                if (child != null)
                {
                    child.Start(beginState);
                }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternAndStart(); }
        }

        public override bool IsFilterStateNode
        {
            get { return false; }
        }

        public override bool IsNotOperator
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

        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternAndEvaluateTrue(_evalAndNode, matchEvent); }

            int? indexFrom = null;
            for (int i = 0; i < _activeChildNodes.Count; i++)
            {
                if (_activeChildNodes[i] == fromNode)
                {
                    indexFrom = i;
                }
            }

            // If one of the children quits, remove the child
            if (isQuitted && indexFrom != null)
            {
                _activeChildNodes[indexFrom.Value] = null;
            }

            if (_eventsPerChild == null || indexFrom == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternAndEvaluateTrue(true); }
                return;
            }

            // If all nodes have events received, the AND expression turns true
            var allHaveEventsExcludingFromChild = true;
            for (int i = 0; i < _eventsPerChild.Length; i++)
            {
                if (indexFrom != i && _eventsPerChild[i] == null)
                {
                    allHaveEventsExcludingFromChild = false;
                    break;
                }
            }

            // if we don't have events from all child nodes, add event and done
            if (!allHaveEventsExcludingFromChild)
            {
                AddMatchEvent(_eventsPerChild, indexFrom.Value, matchEvent);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternAndEvaluateTrue(false); }
                return;
            }

            // if all other nodes have quit other then the from-node, don't retain matching event
            var allOtherNodesQuit = true;
            var hasActive = false;
            for (int i = 0; i < _eventsPerChild.Length; i++)
            {
                if (_activeChildNodes[i] != null)
                {
                    hasActive = true;
                    if (i != indexFrom)
                    {
                        allOtherNodesQuit = false;
                    }
                }
            }

            // if not all other nodes have quit, add event to received list
            if (!allOtherNodesQuit)
            {
                AddMatchEvent(_eventsPerChild, indexFrom.Value, matchEvent);
            }

            // For each combination in eventsPerChild for all other state nodes generate an event to the parent
            List<MatchedEventMap> result = GenerateMatchEvents(matchEvent, _eventsPerChild, indexFrom.Value);

            // Check if this is quitting
            bool quitted = true;
            if (hasActive)
            {
                foreach (EvalStateNode stateNode in _activeChildNodes)
                {
                    if (stateNode != null && !(stateNode.IsNotOperator))
                    {
                        quitted = false;
                    }
                }
            }

            // So we are quitting if all non-not child nodes have quit, since the not-node wait for evaluate false
            if (quitted)
            {
                QuitInternal();
            }

            // Send results to parent
            foreach (MatchedEventMap theEvent in result)
            {
                ParentEvaluator.EvaluateTrue(theEvent, this, quitted);
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternAndEvaluateTrue(_eventsPerChild == null); }
        }

        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternAndEvaluateFalse(_evalAndNode); }
            int? indexFrom = null;
            for (int i = 0; i < _activeChildNodes.Count; i++)
            {
                if (_activeChildNodes[i] == fromNode)
                {
                    _activeChildNodes[i] = null;
                    indexFrom = i;
                }
            }

            if (indexFrom != null)
            {
                _eventsPerChild[indexFrom.Value] = null;
            }

            // The and node cannot turn true anymore, might as well quit all child nodes
            QuitInternal();
            ParentEvaluator.EvaluateFalse(this, restartable ? true : false);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternAndEvaluateFalse(); }
        }

        /// <summary>
        /// Generate a list of matching event combinations constisting of the events per child that are passed in.
        /// </summary>
        /// <param name="matchEvent">can be populated with prior events that must be passed on</param>
        /// <param name="eventsPerChild">is the list of events for each child node to the "And" node.</param>
        /// <param name="indexFrom">The index from.</param>
        /// <returns>
        /// list of events populated with all possible combinations
        /// </returns>
        public static List<MatchedEventMap> GenerateMatchEvents(MatchedEventMap matchEvent,
                                                                Object[] eventsPerChild,
                                                                int indexFrom)
        {
            // Place event list for each child state node into an array, excluding the node where the event came from
            var listArray = new List<IList<MatchedEventMap>>();
            int index = 0;
            for (int i = 0; i < eventsPerChild.Length; i++)
            {
                var eventsChild = eventsPerChild[i];
                if (indexFrom != i && eventsChild != null)
                {
                    if (eventsChild is MatchedEventMap)
                    {
                        listArray.Insert(index++, Collections.SingletonList((MatchedEventMap)eventsChild));
                    }
                    else
                    {
                        listArray.Insert(index++, (IList<MatchedEventMap>)eventsChild);
                    }
                }
            }

            // Recusively generate MatchedEventMap instances for all accumulated events
            var results = new List<MatchedEventMap>();
            GenerateMatchEvents(listArray, 0, results, matchEvent);

            return results;
        }

        /// <summary>For each combination of MatchedEventMap instance in all collections, add an entry to the list. Recursive method. </summary>
        /// <param name="eventList">is an array of lists containing MatchedEventMap instances to combine</param>
        /// <param name="index">is the current index into the array</param>
        /// <param name="result">is the resulting list of MatchedEventMap</param>
        /// <param name="matchEvent">is the start MatchedEventMap to generate from</param>
        internal static void GenerateMatchEvents(
            IList<IList<MatchedEventMap>> eventList,
            int index,
            IList<MatchedEventMap> result,
            MatchedEventMap matchEvent)
        {
            IList<MatchedEventMap> events = eventList[index];

            foreach (MatchedEventMap theEvent in events)
            {
                MatchedEventMap current = matchEvent.ShallowCopy();
                current.Merge(theEvent);

                // If this is the very last list in the array of lists, add accumulated MatchedEventMap events to result
                if ((index + 1) == eventList.Count)
                {
                    result.Add(current);
                }
                else
                {
                    // make a copy of the event collection and hand to next list of events
                    GenerateMatchEvents(eventList, index + 1, result, current);
                }
            }
        }

        public override void Quit()
        {
            if (_eventsPerChild == null)
            {
                return;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternAndQuit(_evalAndNode); }
            QuitInternal();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternAndQuit(); }
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitAnd(_evalAndNode.FactoryNode, this, _eventsPerChild);
            foreach (EvalStateNode node in _activeChildNodes)
            {
                if (node != null)
                {
                    node.Accept(visitor);
                }
            }
        }

        public override String ToString()
        {
            return "EvalAndStateNode";
        }

        public static void AddMatchEvent(Object[] eventsPerChild, int indexFrom, MatchedEventMap matchEvent)
        {
            var matchEventHolder = eventsPerChild[indexFrom];
            if (matchEventHolder == null)
            {
                eventsPerChild[indexFrom] = matchEvent;
            }
            else if (matchEventHolder is MatchedEventMap)
            {
                var list = new List<MatchedEventMap>(4);
                list.Add((MatchedEventMap)matchEventHolder);
                list.Add(matchEvent);
                eventsPerChild[indexFrom] = list;
            }
            else
            {
                var list = (IList<MatchedEventMap>)matchEventHolder;
                list.Add(matchEvent);
            }
        }

        private void QuitInternal()
        {
            foreach (EvalStateNode child in _activeChildNodes)
            {
                if (child != null)
                {
                    child.Quit();
                }
            }

            _activeChildNodes.Fill((EvalStateNode)null);
            _eventsPerChild = null;
        }
    }
}
