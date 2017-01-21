///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents the state of a followed-by operator in the evaluation state tree.
    /// </summary>
    public sealed class EvalAuditStateNode : EvalStateNode, Evaluator
    {
        private readonly EvalAuditNode _evalAuditNode;
        private readonly EvalStateNode _childState;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentNode">is the parent evaluator to call to indicate truth value</param>
        /// <param name="evalAuditNode">is the factory node associated to the state</param>
        /// <param name="stateNodeNumber">The state node number.</param>
        /// <param name="stateNodeId">The state node id.</param>
        public EvalAuditStateNode(Evaluator parentNode,
                                  EvalAuditNode evalAuditNode,
                                  EvalStateNodeNumber stateNodeNumber,
                                  long stateNodeId)
            : base(parentNode)
        {
            _evalAuditNode = evalAuditNode;
            _childState = evalAuditNode.ChildNode.NewState(this, stateNodeNumber, stateNodeId);
        }

        public override void RemoveMatch(ISet<EventBean> matchEvent)
        {
            if (_childState != null)
            {
                _childState.RemoveMatch(matchEvent);
            }
        }

        public override EvalNode FactoryNode
        {
            get { return _evalAuditNode; }
        }

        public override void Start(MatchedEventMap beginState)
        {
            _childState.Start(beginState);
            _evalAuditNode.FactoryNode.IncreaseRefCount(this, _evalAuditNode.Context.PatternContext);
        }

        public void EvaluateTrue(MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            if (_evalAuditNode.FactoryNode.IsAuditPattern && AuditPath.IsInfoEnabled) {
                String message = ToStringEvaluateTrue(this, _evalAuditNode.FactoryNode.PatternExpr, matchEvent, fromNode, isQuitted);
                AuditPath.AuditLog(_evalAuditNode.Context.StatementContext.EngineURI, _evalAuditNode.Context.PatternContext.StatementName, AuditEnum.PATTERN, message);
            }

            ParentEvaluator.EvaluateTrue(matchEvent, this, isQuitted);

            if (isQuitted)
            {
                _evalAuditNode.FactoryNode.DecreaseRefCount(this, _evalAuditNode.Context.PatternContext);
            }
        }

        public void EvaluateFalse(EvalStateNode fromNode, bool restartable)
        {
            if (_evalAuditNode.FactoryNode.IsAuditPattern && AuditPath.IsInfoEnabled) {
                String message = ToStringEvaluateFalse(this, _evalAuditNode.FactoryNode.PatternExpr, fromNode);
                AuditPath.AuditLog(_evalAuditNode.Context.StatementContext.EngineURI, _evalAuditNode.Context.PatternContext.StatementName, AuditEnum.PATTERN, message);
            }

            _evalAuditNode.FactoryNode.DecreaseRefCount(this, _evalAuditNode.Context.PatternContext);
            ParentEvaluator.EvaluateFalse(this, restartable);
        }

        public override void Quit()
        {
            if (_childState != null) {
                _childState.Quit();
            }
            _evalAuditNode.FactoryNode.DecreaseRefCount(this, _evalAuditNode.Context.PatternContext);
        }

        public override void Accept(EvalStateNodeVisitor visitor)
        {
            visitor.VisitAudit();
            if (_childState != null) {
                _childState.Accept(visitor);
            }
        }

        public EvalStateNode ChildState
        {
            get { return _childState; }
        }

        public override String ToString()
        {
            return "EvalAuditStateNode";
        }

        public override bool IsNotOperator
        {
            get
            {
                EvalNode evalNode = _evalAuditNode.ChildNode;
                return evalNode is EvalNotNode;
            }
        }

        public bool IsFilterChildNonQuitting
        {
            get { return _evalAuditNode.FactoryNode.IsFilterChildNonQuitting; }
        }

        public override bool IsFilterStateNode
        {
            get { return _evalAuditNode.ChildNode is EvalFilterNode; }
        }

        public override bool IsObserverStateNodeNonRestarting
        {
            get
            {
                if (_childState != null)
                {
                    return _childState.IsObserverStateNodeNonRestarting;
                }
                return false;
            }
        }

        private static String ToStringEvaluateTrue(EvalAuditStateNode current, String patternExpression, MatchedEventMap matchEvent, EvalStateNode fromNode, bool isQuitted)
        {
            var writer = new StringWriter();

            WritePatternExpr(current, patternExpression, writer);
            writer.Write(" evaluate-true {");

            writer.Write(" from: ");
            TypeHelper.WriteInstance(writer, fromNode, false);

            writer.Write(" map: {");
            var delimiter = "";
            var data = matchEvent.MatchingEvents;
            for (int i = 0; i < data.Length; i++) {
                var name = matchEvent.Meta.TagsPerIndex[i];
                var value = matchEvent.GetMatchingEventAsObject(i);
                writer.Write(delimiter);
                writer.Write(name);
                writer.Write("=");
                if (value is EventBean) {
                    writer.Write(((EventBean) value).Underlying.ToString());
                }
                else if (value is EventBean[]) {
                    writer.Write(EventBeanUtility.Summarize((EventBean[]) value));
                }
                delimiter = ", ";
            }

            writer.Write("} quitted: ");
            writer.Write(isQuitted);

            writer.Write("}");
            return writer.ToString();
        }

        private String ToStringEvaluateFalse(EvalAuditStateNode current, String patternExpression, EvalStateNode fromNode)
        {
            var writer = new StringWriter();
            WritePatternExpr(current, patternExpression, writer);
            writer.Write(" evaluate-false {");

            writer.Write(" from ");
            TypeHelper.WriteInstance(writer, fromNode, false);

            writer.Write("}");
            return writer.ToString();
        }

        internal static void WritePatternExpr(EvalAuditStateNode current, String patternExpression, TextWriter writer)
        {
            if (patternExpression != null)
            {
                writer.Write('(');
                writer.Write(patternExpression);
                writer.Write(')');
            }
            else
            {
                TypeHelper.WriteInstance(writer, "subexr", current);
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
