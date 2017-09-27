///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Method to call to destroy an EPStatement.
    /// </summary>
    public class EPStatementDestroyCallbackList : EPStatementDestroyMethod
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Deque<DestroyCallback> _callbacks;

        public void AddCallback(Action destroyAction)
        {
            AddCallback(new ProxyDestroyCallback(destroyAction));
        }

        public void AddCallback(DestroyCallback destroyCallback)
        {
            if (_callbacks == null)
            {
                _callbacks = new ArrayDeque<DestroyCallback>(2);
            }
            _callbacks.Add(destroyCallback);
        }

        public void Destroy()
        {
            if (_callbacks != null)
            {
                _callbacks.Visit(
                    destroyCallback =>
                    {
                        try
                        {
                            destroyCallback.Destroy();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Failed to destroy resource: " + ex.Message, ex);
                        }
                    });
            }
        }
    }
}
