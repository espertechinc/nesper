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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents the state of a "or" operator in the evaluation state tree.
    /// </summary>
    public class EvalOrStateNode : EvalStateNode, Evaluator
    {
        protected readonly EvalOrNode EvalOrNode;
        protected readonly EvalStateNode[] ChildNodes;
    
        /// <summary>Constructor. </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalOrNode">is the factory node associated to the state</param>
        public EvalOrStateNode(Evaluator parentNode, EvalOrNode evalOrNode) 
            : base(parentNode)
        {
            ChildNodes = new EvalStateNode[evalOrNode.ChildNodes.Length];
            EvalOrNode = evalOrNode;
        }
    
        public override void RemoveMatch(ISet<EventBean> matchEvent) {
            foreach (EvalStateNode node in ChildNodes) {
                if (node != null) {
                    node.RemoveMatch(matchEvent);
                }
            }
        }

        public override EvalNode FactoryNode
        {
            get { return EvalOrNode; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternOrStart(EvalOrNode, beginState);}
            // In an "or" expression we need to create states for all child expressions/listeners,
            // since all are going to be started
            int count = 0;
            foreach (EvalNode node in EvalOrNode.ChildNodes)
            {
                EvalStateNode childState = node.NewState(this, null, 0L);
                ChildNodes[count++] = childState;
            }
    
            // In an "or" expression we start all child listeners
            EvalStateNode[] childNodeCopy = new EvalStateNode[ChildNodes.Length];
            Array.Copy(ChildNodes, 0, childNodeCopy, 0, ChildNodes.Length);
            foreach (EvalStateNode child in childNodeCopy)
            {
                child.Start(beginState);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternOrStart();}
        }
    
        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternOrEvaluateTrue(EvalOrNode, matchEvent);}
            // If one of the children quits, the whole or expression turns true and all subexpressions must quit
            if (isQuitted)
            {
                for (int i = 0; i < ChildNodes.Length; i++) {
                    if (ChildNodes[i] == fromNode) {
                        ChildNodes[i] = null;
                    }
                }
                QuitInternal();     // Quit the remaining listeners
            }
    
            ParentEvaluator.EvaluateTrue(matchEvent, this, isQuitted);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternOrEvaluateTrue(isQuitted);}
        }
    
        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternOrEvalFalse(EvalOrNode);}
            for (int i = 0; i < ChildNodes.Length; i++) {
                if (ChildNodes[i] == fromNode) {
                    ChildNodes[i] = null;
                }
            }
    
            bool allEmpty = true;
            for (int i = 0; i < ChildNodes.Length; i++) {
                if (ChildNodes[i] != null) {
                    allEmpty = false;
                    break;
                }
            }
    
            if (allEmpty) {
                ParentEvaluator.EvaluateFalse(this, true);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternOrEvalFalse();}
        }
    
        public override void Quit()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QPatternOrQuit(EvalOrNode);}
            QuitInternal();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().APatternOrQuit();}
        }
    
        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitOr(EvalOrNode.FactoryNode, this);
            foreach (EvalStateNode node in ChildNodes) {
                if (node != null) {
                    node.Accept(visitor);
                }
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
            return "EvalOrStateNode";
        }
    
        private void QuitInternal()
        {
            foreach (EvalStateNode child in ChildNodes)
            {
                if (child != null) {
                    child.Quit();
                }
            }
            ChildNodes.Fill(null);
        }
    }
}
