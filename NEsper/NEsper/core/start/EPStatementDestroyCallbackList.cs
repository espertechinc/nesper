///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.core.start
{
    /// <summary>Method to call to destroy an EPStatement.</summary>
    public class EPStatementDestroyCallbackList : EPStatementDestroyMethod {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private Deque<DestroyCallback> callbacks;
    
        public void AddCallback(DestroyCallback destroyCallback) {
            if (callbacks == null) {
                callbacks = new ArrayDeque<DestroyCallback>(2);
            }
            callbacks.Add(destroyCallback);
        }
    
        public void Destroy() {
            if (callbacks == null) {
                return;
            }
            foreach (DestroyCallback destroyCallback in callbacks) {
                try {
                    destroyCallback.Destroy();
                } catch (RuntimeException ex) {
                    Log.Error("Failed to destroy resource: " + ex.Message, ex);
                }
            }
        }
    }
} // end of namespace
