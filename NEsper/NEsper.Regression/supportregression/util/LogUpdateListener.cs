///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.supportregression.util
{
    public class LogUpdateListener
    {
        private readonly String _fieldNameLogged;
    
        public LogUpdateListener(String fieldNameLogged)
        {
            _fieldNameLogged = fieldNameLogged;
        }

        public void Update(Object sender, UpdateEventArgs e)
        {
            EventBean theEvent = e.NewEvents[0];
            if (_fieldNameLogged == null)
            {
                ThreadLogUtil.Trace("listener received, " + " listener=" + this + " eventUnderlying=" +
                                    theEvent.Underlying.GetHashCode().ToString("X2"));
            }
            else
            {
                ThreadLogUtil.Trace("listener received, " + " listener=" + this + " eventUnderlying=" +
                                    theEvent.Get(_fieldNameLogged).GetHashCode().ToString("X2"));
            }
        }
    }
}
