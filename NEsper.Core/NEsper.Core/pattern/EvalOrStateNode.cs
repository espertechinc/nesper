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
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents the state of a "or" operator in the evaluation state tree.
    /// </summary>
    public class EvalOrStateNode : EvalStateNode, Evaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EvalOrNode _evalOrNode;
        private readonly EvalStateNode[] _childNodes;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalOrNode">is the factory node associated to the state</param>
        public EvalOrStateNode(Evaluator parentNode, EvalOrNode evalOrNode)
            :  base(parentNode)
        {
            _childNodes = new EvalStateNode[evalOrNode.ChildNodes.Count];
            _evalOrNode = evalOrNode;
        }
    
        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            foreach (EvalStateNode node in _childNodes)
            {
                if (node != null)
                {
                    node.RemoveMatch(matchEvent);
                }
            }
        }

        public override EvalNode FactoryNode
        {
            get { return _evalOrNode; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QPatternOrStart(_evalOrNode, beginState);
            }
            // In an "or" expression we need to create states for all child expressions/listeners,
            // since all are going to be started
            int count = 0;
            foreach (EvalNode node in _evalOrNode.ChildNodes) {
                EvalStateNode childState = node.NewState(this, null, 0L);
                _childNodes[count++] = childState;
            }
    
            // In an "or" expression we start all child listeners
            var childNodeCopy = new EvalStateNode[_childNodes.Length];
            Array.Copy(_childNodes, 0, childNodeCopy, 0, _childNodes.Length);
            foreach (EvalStateNode child in childNodeCopy) {
                child.Start(beginState);
            }
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().APatternOrStart();
            }
        }
    
        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted) {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QPatternOrEvaluateTrue(_evalOrNode, matchEvent);
            }
            // If one of the children quits, the whole or expression turns true and all subexpressions must quit
            if (isQuitted) {
                for (int i = 0; i < _childNodes.Length; i++) {
                    if (_childNodes[i] == fromNode) {
                        _childNodes[i] = null;
                    }
                }
                QuitInternal();     // Quit the remaining listeners
            }
    
            this.ParentEvaluator.EvaluateTrue(matchEvent, this, isQuitted);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().APatternOrEvaluateTrue(isQuitted);
            }
        }
    
        public void EvaluateFalse(EvalStateNode fromNode, bool restartable) {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QPatternOrEvalFalse(_evalOrNode);
            }
            for (int i = 0; i < _childNodes.Length; i++) {
                if (_childNodes[i] == fromNode) {
                    _childNodes[i] = null;
                }
            }
    
            bool allEmpty = true;
            for (int i = 0; i < _childNodes.Length; i++) {
                if (_childNodes[i] != null) {
                    allEmpty = false;
                    break;
                }
            }
    
            if (allEmpty) {
                this.ParentEvaluator.EvaluateFalse(this, true);
            }
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().APatternOrEvalFalse();
            }
        }
    
        public override void Quit() {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QPatternOrQuit(_evalOrNode);
            }
            QuitInternal();
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().APatternOrQuit();
            }
        }
    
        public override void Accept(EvalStateNodeVisitor visitor) {
            visitor.VisitOr(_evalOrNode.FactoryNode, this);
            foreach (EvalStateNode node in _childNodes) {
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

        public override String ToString() {
            return "EvalOrStateNode";
        }
    
        private void QuitInternal() {
            foreach (EvalStateNode child in _childNodes) {
                if (child != null) {
                    child.Quit();
                }
            }
            CompatExtensions.Fill(_childNodes, (EvalStateNode) null);
        }
    }
} // end of namespace
