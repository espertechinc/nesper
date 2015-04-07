///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.service;

namespace com.espertech.esper.core.context.activator
{
    public class ViewableActivatorFilterProxyStopCallback 
    {
        private readonly ViewableActivatorFilterProxy _parent;
        private EPStatementHandleCallback _filterHandle;
    
        public ViewableActivatorFilterProxyStopCallback(ViewableActivatorFilterProxy parent, EPStatementHandleCallback filterHandle) {
            _parent = parent;
            _filterHandle = filterHandle;
        }
    
        public void Stop() {
            lock (this)
            {
                if (_filterHandle != null)
                {
                    _parent.Services.FilterService.Remove(_filterHandle);
                }
                _filterHandle = null;
            }
        }
    }
}
