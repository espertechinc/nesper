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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.funcs
{
    /// <summary>
    /// Represents the TYPEOF(a) function is an expression tree.
    /// </summary>
    [Serializable]
    public class ExprTypeofNode
        : ExprNodeBase
        , ExprFilterOptimizableNode
    {
        [NonSerialized]
        private ExprEvaluator _evaluator;

        public override ExprEvaluator ExprEvaluator
        {
            get { return _evaluator; }
        }

        public IDictionary<string, object> EventType
        {
            get { return null; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count != 1)
            {
                throw new ExprValidationException("Typeof node must have 1 child expression node supplying the expression to test");
            }

            if (ChildNodes[0] is ExprStreamUnderlyingNode)
            {
                var stream = (ExprStreamUnderlyingNode)ChildNodes[0];
                _evaluator = new StreamEventTypeEval(stream.StreamId);
                return null;
            }

            if (ChildNodes[0] is ExprIdentNode)
            {
                var ident = (ExprIdentNode)ChildNodes[0];
                var streamNum = validationContext.StreamTypeService.GetStreamNumForStreamName(ident.FullUnresolvedName);
                if (streamNum != -1)
                {
                    _evaluator = new StreamEventTypeEval(streamNum);
                    return null;
                }

                var eventType = validationContext.StreamTypeService.EventTypes[ident.StreamId];
                if (eventType.GetFragmentType(ident.ResolvedPropertyName) != null)
                {
                    _evaluator = new FragmentTypeEval(ident.StreamId, eventType, ident.ResolvedPropertyName);
                    return null;
                }
            }

            _evaluator = new InnerEvaluator(ChildNodes[0].ExprEvaluator);
            return null;
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { return typeof(string); }
        }

        public bool IsFilterLookupEligible
        {
            get { return true; }
        }

        public FilterSpecLookupable FilterLookupable
        {
            get
            {
                EventPropertyGetter getter = new ProxyEventPropertyGetter
                {
                    ProcGet = eventBean => eventBean.EventType.Name,
                    ProcIsExistsProperty = eventBean => true,
                    ProcGetFragment = eventBean => null
                };
                return new FilterSpecLookupable(
                    this.ToExpressionStringMinPrecedenceSafe(), getter, typeof(string), true);
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("typeof(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprTypeofNode;
        }

        public class StreamEventTypeEval : ExprEvaluator
        {
            private readonly int _streamNum;

            public StreamEventTypeEval(int streamNum)
            {
                _streamNum = streamNum;
            }

            public object Evaluate(EvaluateParams evaluateParams)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprTypeof(); }
                var theEvent = evaluateParams.EventsPerStream[_streamNum];
                if (theEvent == null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTypeof(null); }
                    return null;
                }
                if (theEvent is VariantEvent)
                {
                    var typeName = ((VariantEvent)theEvent).UnderlyingEventBean.EventType.Name;
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTypeof(typeName); }
                    return typeName;
                }
                if (InstrumentationHelper.ENABLED)
                {
                    var typeName = theEvent.EventType.Name;
                    InstrumentationHelper.Get().AExprTypeof(typeName);
                    return typeName;
                }
                return theEvent.EventType.Name;
            }

            public Type ReturnType
            {
                get { return typeof(string); }
            }
        }

        public class FragmentTypeEval : ExprEvaluator
        {
            private readonly int _streamId;
            private readonly EventPropertyGetter _getter;
            private readonly string _fragmentType;

            public FragmentTypeEval(int streamId, EventType eventType, string resolvedPropertyName)
            {
                _streamId = streamId;
                _getter = eventType.GetGetter(resolvedPropertyName);
                _fragmentType = eventType.GetFragmentType(resolvedPropertyName).FragmentType.Name;
            }

            public object Evaluate(EvaluateParams evaluateParams)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprTypeof(); }
                var theEvent = evaluateParams.EventsPerStream[_streamId];
                if (theEvent == null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTypeof(null); }
                    return null;
                }
                var fragment = _getter.GetFragment(theEvent);
                if (fragment == null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTypeof(null); }
                    return null;
                }
                if (fragment is EventBean)
                {
                    var bean = ((EventBean)fragment);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTypeof(bean.EventType.Name); }
                    return bean.EventType.Name;
                }
                if (fragment.GetType().IsArray)
                {
                    var type = _fragmentType + "[]";
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTypeof(type); }
                    return type;
                }
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTypeof(null); }
                return null;
            }

            public Type ReturnType
            {
                get { return typeof(string); }
            }
        }

        private class InnerEvaluator : ExprEvaluator
        {
            private readonly ExprEvaluator _evaluator;

            public InnerEvaluator(ExprEvaluator evaluator)
            {
                _evaluator = evaluator;
            }

            public Type ReturnType
            {
                get { return typeof(string); }
            }

            public object Evaluate(EvaluateParams evaluateParams)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprTypeof(); }
                var result = _evaluator.Evaluate(evaluateParams);
                if (result == null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTypeof(null); }
                    return null;
                }
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprTypeof(result.GetType().Name); }
                return result.GetType().Name;
            }
        }
    }
} // end of namespace
