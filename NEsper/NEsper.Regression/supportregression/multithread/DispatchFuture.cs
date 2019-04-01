///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.dispatch;

namespace com.espertech.esper.supportregression.multithread
{
    /// <summary>
    /// DF3   <-->   DF2  <-->  DF1  DF1 completes: set DF2.earlier = null, notify DF2
    /// </summary>
    public class DispatchFuture : Dispatchable
    {
        private UpdateDispatchViewModel _view;
        private DispatchFuture _earlier;
        private DispatchFuture _later;
        [NonSerialized] private bool _isCompleted;
    
        public DispatchFuture(UpdateDispatchViewModel view, DispatchFuture earlier)
        {
            this._view = view;
            this._earlier = earlier;
        }
    
        public DispatchFuture()
        {
            _isCompleted = true;
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
        }

        public DispatchFuture Later
        {
            set { this._later = value; }
        }

        public void Execute()
        {
            while(!_earlier._isCompleted)
            {
                lock(this) {
                    Monitor.Wait(this, 1000);
                }
            }
    
            _view.Execute();
            _isCompleted = true;
    
            if (_later != null)
            {
                lock(_later)
                {
                    Monitor.PulseAll(_later);
                }
            }
            _earlier = null;
            _later = null;
        }
    }
}
