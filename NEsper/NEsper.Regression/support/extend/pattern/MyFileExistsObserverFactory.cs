///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.regressionlib.support.extend.pattern
{
    public class MyFileExistsObserverFactory : ObserverFactory
    {
        protected MatchedEventConvertor convertor;
        protected ExprEvaluator filenameExpression;

        public ExprEvaluator FilenameExpression => filenameExpression;

        public MatchedEventConvertor Convertor => convertor;

        public EventObserver MakeObserver(
            PatternAgentInstanceContext context,
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            object observerState,
            bool isFilterChildNonQuitting)
        {
            var events = convertor == null ? null : convertor.Invoke(beginState);
            var filename = PatternExpressionUtil.EvaluateChecked(
                "File-exists observer ",
                filenameExpression,
                events,
                context.AgentInstanceContext);
            if (filename == null) {
                throw new EPException("Filename evaluated to null");
            }

            return new MyFileExistsObserver(beginState, observerEventEvaluator, filename.ToString());
        }

        public bool IsNonRestarting => false;

        public void SetFilenameExpression(ExprEvaluator filenameExpression)
        {
            this.filenameExpression = filenameExpression;
        }

        public void SetConvertor(MatchedEventConvertor convertor)
        {
            this.convertor = convertor;
        }
    }
} // end of namespace