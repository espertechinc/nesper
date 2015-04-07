///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.dispatch;

namespace com.espertech.esper.multithread.dispatchmodel
{
    /// <summary>
    /// DF3   <-->   DF2  <-->  DF1  DF1 completes: set DF2.earlier = null, notify DF2
    /// </summary>
    public class DispatchFuture : Dispatchable
    {
        private UpdateDispatchViewModel view;
        private DispatchFuture earlier;
        private DispatchFuture later;
        [NonSerialized] private bool isCompleted;
    
        public DispatchFuture(UpdateDispatchViewModel view, DispatchFuture earlier)
        {
            this.view = view;
            this.earlier = earlier;
        }
    
        public DispatchFuture()
        {
            isCompleted = true;
        }

        public bool IsCompleted
        {
            get { return isCompleted; }
        }

        public DispatchFuture Later
        {
            set { this.later = value; }
        }

        public void Execute()
        {
            while(!earlier.isCompleted)
            {
                lock(this) {
                    Monitor.Wait(this, 1000);
                }
            }
    
            view.Execute();
            isCompleted = true;
    
            if (later != null)
            {
                lock(later)
                {
                    Monitor.PulseAll(later);
                }
            }
            earlier = null;
            later = null;
        }
    }
}
