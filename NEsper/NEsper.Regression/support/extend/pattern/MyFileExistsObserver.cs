///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.regressionlib.support.extend.pattern
{
    public class MyFileExistsObserver : EventObserver
    {
        private readonly string filename;
        private readonly ObserverEventEvaluator observerEventEvaluator;

        public MyFileExistsObserver(
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            string filename)
        {
            BeginState = beginState;
            this.observerEventEvaluator = observerEventEvaluator;
            this.filename = filename;
        }

        public MatchedEventMap BeginState { get; }

        public void StartObserve()
        {
            var file = new FileInfo(filename);
            if (file.Exists) {
                observerEventEvaluator.ObserverEvaluateTrue(BeginState, true);
            }
            else {
                observerEventEvaluator.ObserverEvaluateFalse(true);
            }
        }

        public void StopObserve()
        {
            // this is called when the subexpression quits or the pattern is stopped
            // no action required
        }

        public void Accept(EventObserverVisitor visitor)
        {
        }
    }
} // end of namespace