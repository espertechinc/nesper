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
        private readonly string _filename;
        private readonly ObserverEventEvaluator _observerEventEvaluator;

        public MyFileExistsObserver(
            MatchedEventMap beginState,
            ObserverEventEvaluator observerEventEvaluator,
            string filename)
        {
            BeginState = beginState;
            this._observerEventEvaluator = observerEventEvaluator;
            this._filename = filename;
        }

        public MatchedEventMap BeginState { get; }

        public void StartObserve()
        {
            var file = new FileInfo(_filename);
            if (file.Exists) {
                _observerEventEvaluator.ObserverEvaluateTrue(BeginState, true);
            }
            else {
                _observerEventEvaluator.ObserverEvaluateFalse(true);
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