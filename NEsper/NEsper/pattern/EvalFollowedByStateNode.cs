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
    /// This class represents the state of a followed-by operator in the evaluation state tree.
    /// </summary>
    public class EvalFollowedByStateNode : EvalStateNode, Evaluator
    {
        protected readonly EvalFollowedByNode EvalFollowedByNode;
        protected readonly Dictionary<EvalStateNode, int> Nodes;
    
        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalFollowedByNode">is the factory node associated to the state</param>
        public EvalFollowedByStateNode(Evaluator parentNode, EvalFollowedByNode evalFollowedByNode) 
            : base(parentNode)
        {
            EvalFollowedByNode = evalFollowedByNode;
            Nodes = new Dictionary<EvalStateNode, int>();
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
            Instrument.With(
                i => i.QPatternFollowedByStart(EvalFollowedByNode, beginState),
                i => i.APatternFollowedByStart(),
                () =>
                {
                    EvalNode child = EvalFollowedByNode.ChildNodes[0];
                    EvalStateNode childState = child.NewState(this, null, 0L);
                    Nodes.Put(childState, 0);
                    childState.Start(beginState);
                });
        }
    
        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            int index;
            var hasIndex = Nodes.TryGetValue(fromNode, out index);

            bool[] isFollowedByQuitted = { false };

            using (Instrument.With(
                i => i.QPatternFollowedByEvaluateTrue(EvalFollowedByNode, matchEvent, index),
                i => i.APatternFollowedByEvaluateTrue(isFollowedByQuitted[0])))
            {
                if (isQuitted)
                {
                    Nodes.Remove(fromNode);
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
                    if (Nodes.IsEmpty())
                    {
                        isFollowedByQuitted[0] = true;
                    }

                    ParentEvaluator.EvaluateTrue(matchEvent, this, isFollowedByQuitted[0]);
                }
                    // Else start a new sub-expression for the next-in-line filter
                else
                {
                    EvalNode child = EvalFollowedByNode.ChildNodes[index + 1];
                    EvalStateNode childState = child.NewState(this, null, 0L);
                    Nodes.Put(childState, index + 1);
                    childState.Start(matchEvent);
                }
            }
        }
    
        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            Instrument.With(
                i => i.QPatternFollowedByEvalFalse(EvalFollowedByNode),
                i => i.APatternFollowedByEvalFalse(),
                () =>
                {
                    fromNode.Quit();
                    Nodes.Remove(fromNode);

                    if (Nodes.IsEmpty())
                    {
                        ParentEvaluator.EvaluateFalse(this, true);
                        QuitInternal();
                    }
                });
        }
    
        public override void Quit()
        {
            if (Nodes.IsEmpty())
            {
                return;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternFollowedByQuit(EvalFollowedByNode);}
            QuitInternal();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternFollowedByQuit();}
        }
    
        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitFollowedBy(EvalFollowedByNode.FactoryNode, this, Nodes);
            foreach (EvalStateNode node in Nodes.Keys)
            {
                node.Accept(visitor);
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

        public override String ToString()
        {
            return "EvalFollowedByStateNode nodes=" + Nodes.Count;
        }
    
        private void QuitInternal()
        {
            foreach (EvalStateNode child in Nodes.Keys)
            {
                child.Quit();
            }
            Nodes.Clear();
        }
    }
}
