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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.expression.subquery
{
    /// <summary>
    /// Represents a subselect in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprSubselectAllSomeAnyNode : ExprSubselectNode
    {
        private readonly bool _isNot;
        private readonly bool _isAll;
        private readonly RelationalOpEnum? _relationalOp;
    
        [NonSerialized] private SubselectEvalStrategy _evalStrategy;
        /// <summary>Ctor. </summary>
        /// <param name="statementSpec">is the lookup statement spec from the parser, unvalidated</param>
        /// <param name="not">when NOT</param>
        /// <param name="all">when ALL, false for ANY</param>
        /// <param name="relationalOpEnum">operator</param>
        public ExprSubselectAllSomeAnyNode(StatementSpecRaw statementSpec, bool not, bool all, RelationalOpEnum? relationalOpEnum)
            : base(statementSpec)
        {
            _isNot = not;
            _isAll = all;
            _relationalOp = relationalOpEnum;
        }

        /// <summary>Returns true for not. </summary>
        /// <value>not indicator</value>
        public bool IsNot
        {
            get { return _isNot; }
        }

        /// <summary>Returns true for all. </summary>
        /// <value>all indicator</value>
        public bool IsAll
        {
            get { return _isAll; }
        }

        /// <summary>Returns relational op. </summary>
        /// <value>op</value>
        public RelationalOpEnum? RelationalOp
        {
            get { return _relationalOp; }
        }

        public override Type ReturnType
        {
            get { return typeof (bool?); }
        }

        public override void ValidateSubquery(ExprValidationContext validationContext)
        {
            _evalStrategy = SubselectEvalStrategyFactory.CreateStrategy(this, _isNot, _isAll, !_isAll, _relationalOp);
        }
    
        public override Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _evalStrategy.Evaluate(eventsPerStream, isNewData, matchingEvents, exprEvaluatorContext);
        }

        public override LinkedHashMap<string, object> TypableGetRowProperties
        {
            get { return null; }
        }

        public override Object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext) {
            return null;
        }
    
        public override Object[][] EvaluateTypableMulti(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext) {
            return null;
        }
    
        public override ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext context) {
            return null;
        }
    
        public override ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext) {
            return null;
        }
    
        public override EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId) {
            return null;
        }

        public override Type ComponentTypeCollection
        {
            get { return null; }
        }

        public override EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ICollection<EventBean> matchingEvents, ExprEvaluatorContext exprEvaluatorContext) {
            return null;
        }
    
        public override EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId) {
            return null;
        }
    
        public override EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return null;
        }

        public override bool IsAllowMultiColumnSelect
        {
            get { return false; }
        }
    }
}
