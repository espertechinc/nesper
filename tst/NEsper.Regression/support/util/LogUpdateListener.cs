///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.regressionlib.support.util
{
    public class LogUpdateListener : UpdateListener
    {
        private readonly string fieldNameLogged;

        public LogUpdateListener(string fieldNameLogged)
        {
            this.fieldNameLogged = fieldNameLogged;
        }

        public void Update(
            object sender,
            UpdateEventArgs eventArgs)
        {
            var theEvent = eventArgs.NewEvents[0];
            if (fieldNameLogged == null) {
                ThreadLogUtil.Trace(
                    "listener received, " +
                    " listener=" +
                    this +
                    " eventUnderlying=" +
                    theEvent.Underlying.GetHashCode().ToString("X4"));
            }
            else {
                ThreadLogUtil.Trace(
                    "listener received, " +
                    " listener=" +
                    this +
                    " eventUnderlying=" +
                    theEvent.Get("a").GetHashCode().ToString("X4"));
            }
        }
    }
} // end of namespace