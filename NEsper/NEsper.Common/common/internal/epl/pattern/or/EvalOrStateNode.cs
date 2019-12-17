///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.pattern.or
{
    /// <summary>
    ///     This class represents the state of a "or" operator in the evaluation state tree.
    /// </summary>
    public class EvalOrStateNode : EvalStateNode,
        Evaluator
    {
        private readonly EvalStateNode[] childNodes;
        private readonly EvalOrNode evalOrNode;
        private bool quitted;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalOrNode">is the factory node associated to the state</param>
        public EvalOrStateNode(
            Evaluator parentNode,
            EvalOrNode evalOrNode)
            : base(parentNode)
        {
            childNodes = new EvalStateNode[evalOrNode.ChildNodes.Length];
            this.evalOrNode = evalOrNode;
        }

        public override EvalNode FactoryNode => evalOrNode;

        public override bool IsNotOperator => false;

        public override bool IsFilterStateNode => false;

        public override bool IsObserverStateNodeNonRestarting => false;

        public bool IsFilterChildNonQuitting => false;

        public void EvaluateTrue(
            MatchedEventMap matchEvent,
            EvalStateNode fromNode,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            var agentInstanceContext = evalOrNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternOrEvaluateTrue(evalOrNode.factoryNode, matchEvent);

            // If one of the children quits, the whole or expression turns true and all subexpressions must quit
            if (isQuitted) {
                for (var i = 0; i < childNodes.Length; i++) {
                    if (childNodes[i] == fromNode) {
                        childNodes[i] = null;
                    }
                }

                agentInstanceContext.AuditProvider.PatternInstance(false, evalOrNode.factoryNode, agentInstanceContext);
                QuitInternal(); // Quit the remaining listeners
            }

            agentInstanceContext.AuditProvider.PatternTrue(
                evalOrNode.FactoryNode,
                this,
                matchEvent,
                isQuitted,
                agentInstanceContext);
            ParentEvaluator.EvaluateTrue(matchEvent, this, isQuitted, optionalTriggeringEvent);

            agentInstanceContext.InstrumentationProvider.APatternOrEvaluateTrue(isQuitted);
        }

        public void EvaluateFalse(
            EvalStateNode fromNode,
            bool restartable)
        {
            var agentInstanceContext = evalOrNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternOrEvalFalse(evalOrNode.factoryNode);

            for (var i = 0; i < childNodes.Length; i++) {
                if (childNodes[i] == fromNode) {
                    childNodes[i] = null;
                }
            }

            var allEmpty = true;
            for (var i = 0; i < childNodes.Length; i++) {
                if (childNodes[i] != null) {
                    allEmpty = false;
                    break;
                }
            }

            if (allEmpty) {
                agentInstanceContext.AuditProvider.PatternFalse(evalOrNode.FactoryNode, this, agentInstanceContext);
                agentInstanceContext.AuditProvider.PatternInstance(false, evalOrNode.factoryNode, agentInstanceContext);
                ParentEvaluator.EvaluateFalse(this, true);
            }

            agentInstanceContext.InstrumentationProvider.APatternOrEvalFalse();
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            foreach (var node in childNodes) {
                if (node != null) {
                    node.RemoveMatch(matchEvent);
                }
            }
        }

        public override void Start(MatchedEventMap beginState)
        {
            var agentInstanceContext = evalOrNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternOrStart(evalOrNode.factoryNode, beginState);
            agentInstanceContext.AuditProvider.PatternInstance(true, evalOrNode.factoryNode, agentInstanceContext);

            // In an "or" expression we need to create states for all child expressions/listeners,
            // since all are going to be started
            var count = 0;
            foreach (var node in evalOrNode.ChildNodes) {
                var childState = node.NewState(this);
                childNodes[count++] = childState;
            }

            // In an "or" expression we start all child listeners
            var childNodeCopy = new EvalStateNode[childNodes.Length];
            Array.Copy(childNodes, 0, childNodeCopy, 0, childNodes.Length);
            foreach (var child in childNodeCopy) {
                child.Start(beginState);
                if (quitted) {
                    break;
                }
            }

            agentInstanceContext.InstrumentationProvider.APatternOrStart();
        }

        public override void Quit()
        {
            var agentInstanceContext = evalOrNode.Context.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternOrQuit(evalOrNode.factoryNode);
            agentInstanceContext.AuditProvider.PatternInstance(false, evalOrNode.factoryNode, agentInstanceContext);

            QuitInternal();

            agentInstanceContext.InstrumentationProvider.APatternOrQuit();
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitOr(evalOrNode.FactoryNode, this);
            foreach (var node in childNodes) {
                node?.Accept(visitor);
            }
        }

        public override string ToString()
        {
            return "EvalOrStateNode";
        }

        private void QuitInternal()
        {
            foreach (var child in childNodes) {
                child?.Quit();
            }

            childNodes.Fill((EvalStateNode) null);
            quitted = true;
        }
    }
} // end of namespace