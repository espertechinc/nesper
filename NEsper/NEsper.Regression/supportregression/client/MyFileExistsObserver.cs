///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.IO;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.observer;

namespace com.espertech.esper.supportregression.client
{
    public class MyFileExistsObserver : EventObserver
    {
        private readonly MatchedEventMap _beginState;
        private readonly ObserverEventEvaluator _observerEventEvaluator;
        private readonly String _filename;
    
        public MyFileExistsObserver(MatchedEventMap beginState, ObserverEventEvaluator observerEventEvaluator, String filename)
        {
            _beginState = beginState;
            _observerEventEvaluator = observerEventEvaluator;
            _filename = filename;
        }

        public MatchedEventMap BeginState
        {
            get { return _beginState; }
        }

        public void StartObserve()
        {
            if (File.Exists(_filename))
            {
                _observerEventEvaluator.ObserverEvaluateTrue(_beginState, true);
            }
            else
            {
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
}
