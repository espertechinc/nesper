///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     This class is always the root node in the evaluation state tree representing any activated event expression.
    ///     It hold the handle to a further state node with subnodes making up a whole evaluation state tree.
    /// </summary>
    public class EvalRootStateNode : EvalStateNode,
        Evaluator,
        EvalRootState
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EvalRootStateNode));

        internal readonly EvalRootNode rootNode;
        private PatternMatchCallback callback;
        internal EvalNode rootSingleChildNode;
        internal EvalStateNode topStateNode;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="rootNode">root node</param>
        /// <param name="rootSingleChildNode">is the root nodes single child node</param>
        public EvalRootStateNode(
            EvalRootNode rootNode,
            EvalNode rootSingleChildNode)
            : base(null)
        {
            this.rootNode = rootNode;
            this.rootSingleChildNode = rootSingleChildNode;
        }

        public override EvalNode FactoryNode => rootSingleChildNode;

        public override bool IsFilterStateNode => false;

        public override bool IsNotOperator => false;

        public bool IsFilterChildNonQuitting => false;

        public override bool IsObserverStateNodeNonRestarting => false;

        public EvalStateNode TopStateNode => topStateNode;

        /// <summary>
        ///     Hands the callback to use to indicate matching events.
        /// </summary>
        /// <value>is invoked when the event expressions turns true.</value>
        public PatternMatchCallback Callback {
            set => callback = value;
        }

        public void Stop()
        {
            Quit();
        }

        public void StartRecoverable(
            bool startRecoverable,
            MatchedEventMap beginState)
        {
            Start(beginState);
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (topStateNode != null) {
                topStateNode.RemoveMatch(matchEvent);
            }
        }

        public void EvaluateTrue(
            MatchedEventMap matchEvent,
            EvalStateNode fromNode,
            bool isQuitted,
            EventBean optionalTriggeringEvent)
        {
            var agentInstanceContext = rootNode.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternRootEvaluateTrue(matchEvent);

            if (isQuitted) {
                topStateNode = null;
            }

            callback.MatchFound(matchEvent.MatchingEventsAsMap, optionalTriggeringEvent);
            agentInstanceContext.InstrumentationProvider.APatternRootEvaluateTrue(isQuitted);
        }

        public void EvaluateFalse(
            EvalStateNode fromNode,
            bool restartable)
        {
            var agentInstanceContext = rootNode.AgentInstanceContext;
            agentInstanceContext.InstrumentationProvider.QPatternRootEvalFalse();

            if (topStateNode != null) {
                topStateNode.Quit();
                topStateNode = null;
            }

            agentInstanceContext.InstrumentationProvider.APatternRootEvalFalse();
        }

        public override void Quit()
        {
            rootNode.agentInstanceContext.InstrumentationProvider.QPatternRootQuit();
            if (topStateNode != null) {
                topStateNode.Quit();
            }

            topStateNode = null;
            rootNode.agentInstanceContext.InstrumentationProvider.APatternRootQuit();
        }

        public override void Start(MatchedEventMap beginState)
        {
            rootNode.agentInstanceContext.InstrumentationProvider.QPatternRootStart(beginState);
            topStateNode = rootSingleChildNode.NewState(this);
            topStateNode.Start(beginState);
            rootNode.agentInstanceContext.InstrumentationProvider.APatternRootStart();
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitRoot(this);
            if (topStateNode != null) {
                topStateNode.Accept(visitor);
            }
        }

        public override string ToString()
        {
            return "EvalRootStateNode topStateNode=" + topStateNode;
        }
    }
} // end of namespace