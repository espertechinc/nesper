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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.observer;

namespace com.espertech.esper.supportregression.client
{
    public class MyFileExistsObserverFactory : ObserverFactorySupport
    {
        protected ExprNode FilenameExpression;
        protected MatchedEventConvertor Convertor;

        public override void SetObserverParameters(IList<ExprNode> expressionParameters, MatchedEventConvertor convertor, ExprValidationContext validationContext)
        {
            const string message = "File Exists observer takes a single string filename parameter";
            if (expressionParameters.Count != 1)
            {
                throw new ObserverParameterException(message);
            }
            if (!(expressionParameters[0].ExprEvaluator.ReturnType == typeof(string)))
            {
                throw new ObserverParameterException(message);
            }
    
            FilenameExpression = expressionParameters[0];
            Convertor = convertor;
        }

        public override EventObserver MakeObserver(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            EvalStateNodeNumber stateNodeId,
            Object observerState,
            bool isFilterChildNonQuitting)
        {
            Object filename = PatternExpressionUtil.Evaluate(
                "File-Exists observer ", beginState, FilenameExpression, Convertor, null);
            if (filename == null)
            {
                throw new EPException("Filename evaluated to null");
            }

            return new MyFileExistsObserver(beginState, observerEventEvaluator, filename.ToString());
        }

        public override bool IsNonRestarting
        {
            get { return false; }
        }
    }
}
