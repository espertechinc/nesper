///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents the state of a match-until node in the evaluation state tree.
    /// </summary>
    public class EvalMatchUntilStateNode : EvalStateNode, Evaluator
    {
        private readonly EvalMatchUntilNode _evalMatchUntilNode;
        private MatchedEventMap _beginState;
        private readonly IList<EventBean>[] _matchedEventArrays;
    
        private EvalStateNode _stateMatcher;
        private EvalStateNode _stateUntil;
        private int _numMatches;
        private int? _lowerbounds;
        private int? _upperbounds;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalMatchUntilNode">is the factory node associated to the state</param>
        public EvalMatchUntilStateNode(Evaluator parentNode, EvalMatchUntilNode evalMatchUntilNode)
            : base(parentNode)
        {
            _matchedEventArrays = new IList<EventBean>[evalMatchUntilNode.FactoryNode.TagsArrayed.Length];
            _evalMatchUntilNode = evalMatchUntilNode;
        }
    
        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            bool quit = PatternConsumptionUtil.ContainsEvent(matchEvent, _beginState);
            if (!quit) {
                foreach (List<EventBean> list in _matchedEventArrays) {
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
                ParentEvaluator.EvaluateFalse(this, true);
            }
            else {
                if (_stateMatcher != null) {
                    _stateMatcher.RemoveMatch(matchEvent);
                }
                if (_stateUntil != null) {
                    _stateUntil.RemoveMatch(matchEvent);
                }
            }
        }

        public override EvalNode FactoryNode
        {
            get { return _evalMatchUntilNode; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            Instrument.With(
                i => i.QPatternMatchUntilStart(_evalMatchUntilNode, beginState),
                i => i.APatternMatchUntilStart(),
                () =>
                {
                    _beginState = beginState;

                    EvalNode childMatcher = _evalMatchUntilNode.ChildNodeSub;
                    _stateMatcher = childMatcher.NewState(this, null, 0L);

                    if (_evalMatchUntilNode.ChildNodeUntil != null)
                    {
                        EvalNode childUntil = _evalMatchUntilNode.ChildNodeUntil;
                        _stateUntil = childUntil.NewState(this, null, 0L);
                    }

                    // start until first, it controls the expression
                    // if the same event fires both match and until, the match should not count
                    if (_stateUntil != null)
                    {
                        _stateUntil.Start(beginState);
                    }

                    EvalMatchUntilStateBounds bounds =
                        EvalMatchUntilStateBounds.InitBounds(
                            _evalMatchUntilNode.FactoryNode, beginState, _evalMatchUntilNode.Context);
                    _lowerbounds = bounds.Lowerbounds;
                    _upperbounds = bounds.Upperbounds;

                    if (_stateMatcher != null)
                    {
                        _stateMatcher.Start(beginState);
                    }
                });
        }
    
        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            Instrument.With(
                i => i.QPatternMatchUntilEvaluateTrue(_evalMatchUntilNode, matchEvent, fromNode == _stateUntil),
                i => i.APatternMatchUntilEvaluateTrue(_stateMatcher == null && _stateUntil == null),
                () =>
                {
                    bool isMatcher = false;
                    if (fromNode == _stateMatcher)
                    {
                        // Add the additional tagged events to the list for later posting
                        isMatcher = true;
                        _numMatches++;
                        int[] tags = _evalMatchUntilNode.FactoryNode.TagsArrayed;
                        for (int i = 0; i < tags.Length; i++)
                        {
                            var theEvent = matchEvent.GetMatchingEventAsObject(tags[i]);
                            if (theEvent != null)
                            {
                                if (_matchedEventArrays[i] == null)
                                {
                                    _matchedEventArrays[i] = new List<EventBean>();
                                }
                                if (theEvent is EventBean)
                                {
                                    _matchedEventArrays[i].Add((EventBean) theEvent);
                                }
                                else
                                {
                                    EventBean[] arrayEvents = (EventBean[]) theEvent;
                                    _matchedEventArrays[i].AddAll(arrayEvents);
                                }

                            }
                        }
                    }

                    if (isQuitted)
                    {
                        if (isMatcher)
                        {
                            _stateMatcher = null;
                        }
                        else
                        {
                            _stateUntil = null;
                        }
                    }

                    // handle matcher evaluating true
                    if (isMatcher)
                    {
                        if ((IsTightlyBound) && (_numMatches == _lowerbounds))
                        {
                            QuitInternal();
                            MatchedEventMap consolidated = Consolidate(
                                matchEvent, _matchedEventArrays, _evalMatchUntilNode.FactoryNode.TagsArrayed);
                            ParentEvaluator.EvaluateTrue(consolidated, this, true);
                        }
                        else
                        {
                            // restart or keep started if not bounded, or not upper bounds, or upper bounds not reached
                            bool restart = (!IsBounded) ||
                                           (_upperbounds == null) ||
                                           (_upperbounds > _numMatches);
                            if (_stateMatcher == null)
                            {
                                if (restart)
                                {
                                    EvalNode childMatcher = _evalMatchUntilNode.ChildNodeSub;
                                    _stateMatcher = childMatcher.NewState(this, null, 0L);
                                    _stateMatcher.Start(_beginState);
                                }
                            }
                            else
                            {
                                if (!restart)
                                {
                                    _stateMatcher.Quit();
                                    _stateMatcher = null;
                                }
                            }
                        }
                    }
                    else
                        // handle until-node
                    {
                        QuitInternal();

                        // consolidate multiple matched events into a single event
                        MatchedEventMap consolidated = Consolidate(
                            matchEvent, _matchedEventArrays, _evalMatchUntilNode.FactoryNode.TagsArrayed);

                        if ((_lowerbounds != null) && (_numMatches < _lowerbounds))
                        {
                            ParentEvaluator.EvaluateFalse(this, true);
                        }
                        else
                        {
                            ParentEvaluator.EvaluateTrue(consolidated, this, true);
                        }
                    }
                });
        }
    
        public static MatchedEventMap Consolidate(MatchedEventMap beginState, IList<EventBean>[] matchedEventList, int[] tagsArrayed)
        {
            if (tagsArrayed == null)
            {
                return beginState;
            }
    
            for (int i = 0; i < tagsArrayed.Length; i++)
            {
                if (matchedEventList[i] == null)
                {
                    continue;
                }
                EventBean[] eventsForTag = matchedEventList[i].ToArray();
                beginState.Add(tagsArrayed[i], eventsForTag);
            }
    
            return beginState;
        }
    
        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            Instrument.With(
                i => i.QPatternMatchUntilEvalFalse(_evalMatchUntilNode, fromNode == _stateUntil),
                i => i.APatternMatchUntilEvalFalse(),
                () =>
                {
                    var isMatcher = fromNode == _stateMatcher;
                    if (isMatcher)
                    {
                        _stateMatcher.Quit();
                        _stateMatcher = null;
                    }
                    else
                    {
                        _stateUntil.Quit();
                        _stateUntil = null;
                    }
                    ParentEvaluator.EvaluateFalse(this, true);
                });
        }
    
        public override void Quit()
        {
            if (_stateMatcher == null && _stateUntil == null) {
                return;
            }

            Instrument.With(
                i => i.QPatternMatchUntilQuit(_evalMatchUntilNode),
                i => i.APatternMatchUntilQuit(),
                QuitInternal);
        }
    
        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitMatchUntil(_evalMatchUntilNode.FactoryNode, this, _matchedEventArrays, _beginState);
            if (_stateMatcher != null) {
                _stateMatcher.Accept(visitor);
            }
            if (_stateUntil != null) {
                _stateUntil.Accept(visitor);
            }
        }
    
        public override String ToString()
        {
            return "EvalMatchUntilStateNode";
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
            get { return true; }
        }

        public override bool IsObserverStateNodeNonRestarting
        {
            get { return false; }
        }

        private bool IsTightlyBound
        {
            get { return _lowerbounds != null && _upperbounds != null && _upperbounds.Equals(_lowerbounds); }
        }

        private bool IsBounded
        {
            get { return _lowerbounds != null || _upperbounds != null; }
        }

        private void QuitInternal()
        {
            if (_stateMatcher != null)
            {
                _stateMatcher.Quit();
                _stateMatcher = null;
            }
            if (_stateUntil != null)
            {
                _stateUntil.Quit();
                _stateUntil = null;
            }
        }
    }
}
