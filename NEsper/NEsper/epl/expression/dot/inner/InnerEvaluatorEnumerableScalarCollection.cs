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
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.epl.expression.dot.inner
{
    public class InnerEvaluatorEnumerableScalarCollection : ExprDotEvalRootChildInnerEval
    {
        private readonly ExprEvaluatorEnumeration rootLambdaEvaluator;
        private readonly Type componentType;
    
        public InnerEvaluatorEnumerableScalarCollection(ExprEvaluatorEnumeration rootLambdaEvaluator, Type componentType)
        {
            this.rootLambdaEvaluator = rootLambdaEvaluator;
            this.componentType = componentType;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext);
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return rootLambdaEvaluator.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, exprEvaluatorContext);
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return rootLambdaEvaluator.EvaluateGetROCollectionEvents(eventsPerStream, isNewData, context);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return rootLambdaEvaluator.EvaluateGetROCollectionScalar(eventsPerStream, isNewData, context);
        }

        public EventType EventTypeCollection
        {
            get { return null; }
        }

        public Type ComponentTypeCollection
        {
            get { return componentType; }
        }

        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return null;
        }

        public EventType EventTypeSingle
        {
            get { return null; }
        }

        public EPType TypeInfo
        {
            get { return EPTypeHelper.CollectionOfSingleValue(componentType); }
        }
    }
}
