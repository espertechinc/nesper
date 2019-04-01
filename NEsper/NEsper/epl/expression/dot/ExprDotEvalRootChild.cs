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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot.inner;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.dot
{
	[Serializable]
    public class ExprDotEvalRootChild 
        : ExprEvaluator
        , ExprEvaluatorEnumeration
	{
	    private readonly ExprDotNode _dotNode;
	    private readonly ExprDotEvalRootChildInnerEval _innerEvaluator;
	    private readonly ExprDotEval[] _evalIteratorEventBean;
	    private readonly ExprDotEval[] _evalUnpacking;

	    public ExprDotEvalRootChild(
            bool hasEnumerationMethod,
            ExprDotNode dotNode,
            ExprEvaluator rootNodeEvaluator,
            ExprEvaluatorEnumeration rootLambdaEvaluator,
            EPType typeInfo,
            ExprDotEval[] evalIteratorEventBean,
            ExprDotEval[] evalUnpacking,
            bool checkedUnpackEvent) 
        {
	        _dotNode = dotNode;
	        if (rootLambdaEvaluator != null)
	        {
	            if (typeInfo is EventMultiValuedEPType)
	            {
	                _innerEvaluator = new InnerEvaluatorEnumerableEventCollection(
	                    rootLambdaEvaluator, ((EventMultiValuedEPType) typeInfo).Component);
	            }
	            else if (typeInfo is EventEPType) {
	                _innerEvaluator = new InnerEvaluatorEnumerableEventBean(
	                    rootLambdaEvaluator, ((EventEPType) typeInfo).EventType);
	            }
	            else {
	                _innerEvaluator = new InnerEvaluatorEnumerableScalarCollection(
	                    rootLambdaEvaluator, ((ClassMultiValuedEPType) typeInfo).Component);
	            }
	        }
	        else {
	            if (checkedUnpackEvent) {
	                _innerEvaluator = new InnerEvaluatorScalarUnpackEvent(rootNodeEvaluator);
	            }
	            else {
	                var returnType = rootNodeEvaluator.ReturnType;
	                if (hasEnumerationMethod && returnType.IsArray)
	                {
	                    if (returnType.GetElementType().IsPrimitive)
	                    {
	                        _innerEvaluator = new InnerEvaluatorArrPrimitiveToColl(rootNodeEvaluator);
	                    }
	                    else
	                    {
	                        _innerEvaluator = new InnerEvaluatorArrObjectToColl(rootNodeEvaluator);
	                    }
	                }
	                else if (hasEnumerationMethod && TypeHelper.IsImplementsInterface(returnType, typeof (ICollection<EventBean>)))
	                {
	                    _innerEvaluator = new InnerEvaluatorColl(rootNodeEvaluator);
	                }
	                else
	                {
	                    _innerEvaluator = new InnerEvaluatorScalar(rootNodeEvaluator);
	                }
	            }
	        }
	        _evalUnpacking = evalUnpacking;
	        _evalIteratorEventBean = evalIteratorEventBean;
	    }

	    public virtual Type ReturnType
	    {
	        get { return _evalUnpacking[_evalUnpacking.Length - 1].TypeInfo.GetNormalizedClass(); }
	    }

	    public object Evaluate(EvaluateParams evaluateParams)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprDot(_dotNode);}
	        var inner = _innerEvaluator.Evaluate(evaluateParams);
	        if (inner != null) {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprDotChain(_innerEvaluator.TypeInfo, inner, _evalUnpacking);}
	            inner = ExprDotNodeUtility.EvaluateChain(_evalUnpacking, inner, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprDotChain(); }
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprDot(inner); }
	        return inner;
	    }

	    public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams)
        {
            var inner = _innerEvaluator.EvaluateGetROCollectionEvents(evaluateParams);
	        if (inner != null) {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprDotChain(_innerEvaluator.TypeInfo, inner, _evalUnpacking);}
                inner = ExprDotNodeUtility.EvaluateChain(_evalIteratorEventBean, inner, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext).Unwrap<EventBean>(true);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprDotChain();}
	            return inner;
	        }
	        return null;
	    }

	    public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            var inner = _innerEvaluator.EvaluateGetROCollectionScalar(evaluateParams);
	        if (inner != null) {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprDotChain(_innerEvaluator.TypeInfo, inner, _evalUnpacking);}
                inner = ExprDotNodeUtility.EvaluateChain(_evalIteratorEventBean, inner, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext).Unwrap<object>(true);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprDotChain();}
                return inner;
	        }
	        return null;
	    }

	    public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
	        return _innerEvaluator.EventTypeCollection;
	    }

	    public Type ComponentTypeCollection
	    {
	        get { return _innerEvaluator.ComponentTypeCollection; }
	    }

	    public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
	        return null;
	    }

	    public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
        {
	        return null;
	    }
	}
} // end of namespace
